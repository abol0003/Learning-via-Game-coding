using System;
    using System.Collections.Generic; 
namespace Learning
{
    public class TicTacToeGame
    {
        private readonly char[,] _board= new char[3, 3]; // with readonly board can never change type and size
        private char _currentPlayer= 'X'; // Player X 
        private Random _rnd= new Random(); // assign random with rnd for simplicity

        public TicTacToeGame()
        {
            // Initialize the board with empty spaces
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _board[i, j] = ' ';
                }
            }
        }
        private void DisplayBoard()
        {
            Console.WriteLine("  1 2 3");
            for (int r = 0; r < 3; r++)
            {
                Console.Write((r + 1) + " ");
                for (int c = 0; c < 3; c++)
                {
                    Console.Write(_board[r, c]);
                    if (c < 2) Console.Write("|");
                }
                Console.WriteLine();
                if (r < 2) Console.WriteLine("  -+-+-");
            }
            Console.WriteLine();
        }
        private (int row, int col) Place()
        {
            while (true)
            {
                Console.Write("Enter your move (row col): ");
                var raw = Console.ReadLine()?.Trim() ?? "";

                // Try "r c" format
                var pos = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int r, c;
                bool parsed = false;

                if (pos.Length == 2
                    && int.TryParse(pos[0], out int tr)
                    && int.TryParse(pos[1], out int tc))
                {
                    r = tr - 1;
                    c = tc - 1;
                    parsed = true;
                }
                // Try "rc" format
                else if (pos.Length == 1
                         && pos[0].Length == 2
                         && char.IsDigit(pos[0][0])
                         && char.IsDigit(pos[0][1]))
                {
                    r = pos[0][0] - '1';
                    c = pos[0][1] - '1';
                    parsed = true;
                }
                else
                {
                    Console.WriteLine("Invalid format, please try again.");
                    continue;
                }

                // Validate bounds and vacancy
                if (parsed
                    && r is >= 0 and < 3
                    && c is >= 0 and < 3
                    && _board[r, c] == ' ')
                {
                    return (r, c);
                }

                Console.WriteLine("cell occupied, please try again.");
            }
        }

        public void Play()
        {
            int moves = 0;
            while (true)
            {
                Console.Clear();
                DisplayBoard();
                if (_currentPlayer == 'X')
                {
                    var (row, col) = Place();
                    _board[row, col] = 'X';
                }
                else
                {
                    AiMove();                       // AI move
                }

                moves++;

                if (CheckWinFor(_currentPlayer))
                {
                    Console.Clear();
                    DisplayBoard();
                    Console.WriteLine($"Player {_currentPlayer} wins!");
                    return;
                }

                if (moves == 9)
                {
                    Console.Clear();
                    DisplayBoard();
                    Console.WriteLine("It's a draw!");
                    return;
                }

                // Switch player
                _currentPlayer = _currentPlayer == 'X' ? 'O' : 'X';
            }
        }
        private void AiMove()
        {
            //try to win
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (_board[r, c] == ' ')
                    {
                        _board[r, c] = 'O';
                        if (CheckWinFor('O'))
                        {
                            Console.WriteLine($"AI plays at {r + 1},{c + 1} to win");
                            Thread.Sleep(500);
                            return;
                        }
                        _board[r, c] = ' ';
                    }
                }
            }

            // 2) block opponent from winning
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    if (_board[r, c] == ' ')
                    {
                        _board[r, c] = 'X';
                        if (CheckWinFor('X'))
                        {
                            _board[r, c] = 'O';
                            Console.WriteLine($"AI plays at {r + 1},{c + 1} to block");
                            Thread.Sleep(500);
                            return;
                        }
                        _board[r, c] = ' ';
                    }
                }
            }

            //else random move
            var empties = new List<(int r, int c)>();
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (_board[r, c] == ' ')
                        empties.Add((r, c));

            var (row, col) = empties[_rnd.Next(empties.Count)];
            _board[row, col] = 'O';
            Console.WriteLine($"AI plays at {row + 1},{col + 1}");
            Thread.Sleep(500);
        }

        private bool CheckWinFor(char p)
        {
            // rows/cols
            for (int i = 0; i < 3; i++)
                if ((_board[i, 0] == p && _board[i, 1] == p && _board[i, 2] == p) ||
                    (_board[0, i] == p && _board[1, i] == p && _board[2, i] == p))
                    return true;

            // diagonals
            if ((_board[0, 0] == p && _board[1, 1] == p && _board[2, 2] == p) ||
                (_board[0, 2] == p && _board[1, 1] == p && _board[2, 0] == p))
                return true;

            return false;
        }

    }
}

    


