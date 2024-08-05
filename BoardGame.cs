using System;
using System.Collections.Generic;

namespace ConsoleGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
        }
    }

    class Game
    {
        private const int BoardSize = 10;
        private const int PlayersCount = 4;
        private const int PiecesPerPlayer = 4;
        private readonly Player[] players = new Player[PlayersCount];
        private int currentPlayerIndex = 0;
        private Random dice = new Random();
        private char[,] board = new char[BoardSize, BoardSize];

        public Game()
        {
            for (int i = 0; i < PlayersCount; i++)
            {
                players[i] = new Player(i);
            }

            InitializeBoard();
        }

        public void Start()
        {
            while (true)
            {
                Player currentPlayer = players[currentPlayerIndex];
                Console.Clear();
                DisplayBoard();

                Console.WriteLine($"Player {currentPlayer.Index + 1}'s turn. Press Enter to roll the dice.");
                Console.ReadLine();
                int roll = RollDice();
                Console.WriteLine($"Player {currentPlayer.Index + 1} rolled a {roll}.");

                if (roll == 6 && currentPlayer.HasPiecesInHome())
                {
                    currentPlayer.MovePieceFromHome();
                }
                else if (currentPlayer.HasPiecesOnBoard())
                {
                    currentPlayer.MovePiece(roll, board, players);
                }

                if (currentPlayer.HasWon())
                {
                    Console.WriteLine($"Player {currentPlayer.Index + 1} wins!");
                    break;
                }

                currentPlayerIndex = (currentPlayerIndex + 1) % PlayersCount;
            }
        }

        private int RollDice()
        {
            return dice.Next(1, 7);
        }

        private void InitializeBoard()
        {
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    board[i, j] = '.';
                }
            }

            // Mark the starting positions
            board[0, 0] = '1';
            board[0, BoardSize - 1] = '2';
            board[BoardSize - 1, 0] = '3';
            board[BoardSize - 1, BoardSize - 1] = '4';
        }

        private void DisplayBoard()
        {
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    Console.Write(board[i, j] + " ");
                }
                Console.WriteLine();
            }
        }
    }

    class Player
    {
        public int Index { get; }
        private List<Piece> pieces = new List<Piece>();

        public Player(int index)
        {
            Index = index;
            for (int i = 0; i < 4; i++)
            {
                pieces.Add(new Piece(index));
            }
        }

        public bool HasPiecesInHome()
        {
            return pieces.Exists(p => p.IsInHome());
        }

        public bool HasPiecesOnBoard()
        {
            return pieces.Exists(p => !p.IsInHome() && !p.HasFinished());
        }

        public void MovePieceFromHome()
        {
            Piece piece = pieces.Find(p => p.IsInHome());
            piece.EnterField();
        }

        public void MovePiece(int roll, char[,] board, Player[] players)
        {
            Piece piece = pieces.Find(p => !p.IsInHome() && !p.HasFinished());
            if (piece != null)
            {
                piece.Move(roll, board, players);
            }
        }

        public bool HasWon()
        {
            return pieces.TrueForAll(p => p.HasFinished());
        }

        public List<Piece> GetPieces()
        {
            return pieces;
        }
    }

    class Piece
    {
        private int playerIndex;
        private bool inHome = true;
        private bool finished = false;
        public int x { get; private set; }
        public int y { get; private set; }

        public Piece(int playerIndex)
        {
            this.playerIndex = playerIndex;
        }

        public bool IsInHome()
        {
            return inHome;
        }

        public bool HasFinished()
        {
            return finished;
        }

        public void EnterField()
        {
            inHome = false;
            switch (playerIndex)
            {
                case 0: x = 0; y = 0; break;
                case 1: x = 0; y = 9; break;
                case 2: x = 9; y = 0; break;
                case 3: x = 9; y = 9; break;
            }
        }

        public void Move(int roll, char[,] board, Player[] players)
        {
            // Remove piece from the old position
            board[x, y] = '.';

            for (int i = 0; i < roll; i++)
            {
                MoveOneStep(board);
            }

            // Check if the piece has finished
            if (x == 4 && y == 4)
            {
                finished = true;
            }

            if (!finished)
            {
                // Check if another piece is on the new position
                foreach (var player in players)
                {
                    if (player.Index != playerIndex)
                    {
                        foreach (var piece in player.GetPieces())
                        {
                            if (piece.x == x && piece.y == y)
                            {
                                piece.SendHome();
                            }
                        }
                    }
                }
            }

            // Place piece on the new position
            board[x, y] = (char)('1' + playerIndex);
        }

        private void MoveOneStep(char[,] board)
        {
            if (x == 0 && y < board.GetLength(1) - 1) { y++; }
            else if (y == board.GetLength(1) - 1 && x < board.GetLength(0) - 1) { x++; }
            else if (x == board.GetLength(0) - 1 && y > 0) { y--; }
            else if (y == 0 && x > 0) { x--; }
            else if (x > 0 && x <= board.GetLength(0) / 2 && y == 0) { x--; }
            else if (x == board.GetLength(0) / 2 && y < board.GetLength(1) / 2) { y++; }
        }

        public void SendHome()
        {
            inHome = true;
            x = -1;
            y = -1;
        }
    }
}
