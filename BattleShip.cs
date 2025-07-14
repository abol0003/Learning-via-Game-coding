using System;
using System.Collections.Generic;
using System.Linq;

namespace Learning
{

    class BattleShip
    {
        static void Main()
        {
            var game = new BattleshipGame(); // Initialize game with two boards
            game.Setup();                     // Place fleets randomly
            game.Play();                      // Enter game loop
            Console.WriteLine("Press any key to exit..."); // Final prompt
            Console.ReadKey();                // Wait for user input to close
        }
    }

    public class BattleshipGame
    {
        private readonly Board _playerBoard;
        private readonly Board _aiBoard;
        private readonly Random _rnd = new Random();
        private const int BoardSize = 10;
        private readonly int[] _fleetSizes = { 5, 4, 3, 3, 2 };

        public BattleshipGame()
        {
            // Create two boards of equal size for player and AI
            _playerBoard = new Board(BoardSize);
            _aiBoard = new Board(BoardSize);
        }

        /*
         * Set up both fleets by placing ships randomly.
         */
        public void Setup()
        {
            Console.WriteLine("Placing AI fleet...");
            PlaceFleetRandomly(_aiBoard);

            Console.WriteLine("Placing player fleet...\n");
            PlaceFleetRandomly(_playerBoard);
        }

        /*
         * Main game loop alternates between player and AI turns
         * until one side's ships are all sunk. The loop checks
         * game state after each turn to determine when to exit.
         */
        public void Play()
        {
            while (!_aiBoard.AllShipsSunk && !_playerBoard.AllShipsSunk)
            {
                PlayerTurn();
                if (_aiBoard.AllShipsSunk) break; // End if AI fleet down

                AiTurn(); // AI takes its turn if player hasn't won
            }

            // Display game result based on final board states
            Console.WriteLine(_aiBoard.AllShipsSunk
                ? "Congratulations! You won!"
                : "Game over. The AI won.");
        }

        /*
         * Handle player's turn: display boards, prompt for input,
         * validate and process shot, then pause for feedback.
         */
        private void PlayerTurn()
        {
            Console.Clear();
            Console.WriteLine("-- Player Board --");
            _playerBoard.Display(showShips: true);
            Console.WriteLine("-- AI Board (hidden) --");
            _aiBoard.Display(showShips: false);

            Coordinate coord;
            while (true)
            {
                Console.Write("Enter your shot (e.g. C5): ");
                var input = Console.ReadLine();
                try
                {
                    coord = Coordinate.Parse(input); // Convert input to grid indices
                    if (_aiBoard.AlreadyShot(coord))
                    {
                        Console.WriteLine("You already shot here, try another.");
                        continue;
                    }
                    break;
                }
                catch
                {
                    Console.WriteLine("Invalid coordinate! Please try again.");
                }
            }

            var hit = _aiBoard.Shoot(coord); // Perform shot on AI board
            Console.WriteLine(hit ? "Hit!" : "Miss!");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        /*
         * AI turn: randomly select a cell that hasn't been targeted,
         * perform the shot, show the result.
         */
        private void AiTurn()
        {
            Console.Clear();
            Console.WriteLine("AI is taking its turn...");
            Coordinate coord;
            do
            {
                coord = new Coordinate(
                    _rnd.Next(0, BoardSize),
                    _rnd.Next(0, BoardSize));
            } while (_playerBoard.AlreadyShot(coord));

            var hit = _playerBoard.Shoot(coord);
            Console.WriteLine($"AI shot at {coord}: {(hit ? "Hit!" : "Miss!")}");
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        /*
         * Randomly places ships on a given board, ensuring no overlap.
         * Demonstrates collision detection and random positioning.
         */
        private void PlaceFleetRandomly(Board board)
        {
            foreach (var size in _fleetSizes)
                board.PlaceShipRandomly(size, _rnd);
        }
    }

    public class Board
    {
        private readonly CellState[,] _grid;    // 2D array for cell state
        private readonly List<Ship> _ships = new List<Ship>();
        private readonly int _size;

        public Board(int size)
        {
            _size = size;
            _grid = new CellState[size, size]; // Initialize all cells to Empty
        }

        // True when all ships in _ships are sunk
        public bool AllShipsSunk => _ships.All(s => s.IsSunk);

        /*
         * Prints the board to console. If showShips is true,
         * ship positions are revealed; otherwise they remain hidden.
         */
        public void Display(bool showShips)
        {
            Console.Write("  ");
            for (int c = 1; c <= _size; c++)
                Console.Write(c.ToString().PadLeft(2) + " ");
            Console.WriteLine();

            for (int r = 0; r < _size; r++)
            {
                Console.Write((char)('A' + r) + " ");
                for (int c = 0; c < _size; c++)
                {
                    var cell = _grid[r, c];
                    char symbol = cell switch
                    {
                        CellState.Empty => '.',
                        CellState.Ship => showShips ? 'S' : '.',
                        CellState.Hit => 'X',
                        CellState.Miss => 'M',
                        _ => '?'
                    };
                    Console.Write(" " + symbol + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        // Returns true if the specified coordinate was already targeted
        public bool AlreadyShot(Coordinate c) =>
            _grid[c.X, c.Y] == CellState.Hit || _grid[c.X, c.Y] == CellState.Miss;

        /*
         * Marks the cell as Hit or Miss. If a ship occupies the cell,
         * registers a hit on that ship instance. Returns true if Hit.
         */
        public bool Shoot(Coordinate c)
        {
            if (_grid[c.X, c.Y] == CellState.Ship)
            {
                _grid[c.X, c.Y] = CellState.Hit;
                _ships.First(s => s.Contains(c)).Hit(c);
                return true;
            }
            _grid[c.X, c.Y] = CellState.Miss;
            return false;
        }

        /*
         * Attempts to place a ship of given size at a random location
         * and orientation. Continues until a valid placement (no overlap).
         */
        public void PlaceShipRandomly(int size, Random rnd)
        {
            bool placed = false;
            while (!placed)
            {
                bool horizontal = rnd.Next(2) == 0;
                int maxRow = horizontal ? _size : _size - size;
                int maxCol = horizontal ? _size - size : _size;
                int row = rnd.Next(0, maxRow);
                int col = rnd.Next(0, maxCol);

                var coords = Enumerable.Range(0, size)
                    .Select(i => new Coordinate(
                        row + (horizontal ? 0 : i),
                        col + (horizontal ? i : 0)))
                    .ToList();

                // Check for overlap with existing ships
                if (coords.Any(c => _grid[c.X, c.Y] != CellState.Empty))
                    continue;

                // Record ship and mark grid cells
                var ship = new Ship(coords);
                _ships.Add(ship);
                foreach (var c in coords)
                    _grid[c.X, c.Y] = CellState.Ship;

                placed = true;
            }
        }
    }

    public class Ship
    {
        private readonly List<Coordinate> _positions;
        private readonly HashSet<Coordinate> _hits = new HashSet<Coordinate>();

        // Initialize ship with its list of coordinates
        public Ship(IEnumerable<Coordinate> coords)
        {
            _positions = coords.ToList();
        }

        public bool Contains(Coordinate c) => _positions.Contains(c);

        // Register a hit on this ship if the coordinate matches
        public void Hit(Coordinate c) => _hits.Add(c);

        // All positions hit => ship is sunk
        public bool IsSunk => _hits.Count == _positions.Count;
    }

    public struct Coordinate
    {
        public int X { get; }
        public int Y { get; }

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        /*
         * Converts user input (like "A5") to grid coordinates.
         * Throws FormatException for invalid formats.
         */
        public static Coordinate Parse(string s)
        {
            if (string.IsNullOrWhiteSpace(s) || s.Length < 2)
                throw new FormatException();

            s = s.ToUpper().Trim();
            int x = s[0] - 'A';
            if (x < 0 || x > 25)
                throw new FormatException();

            if (!int.TryParse(s.Substring(1), out int col))
                throw new FormatException();
            int y = col - 1;
            return new Coordinate(x, y);
        }

        public override string ToString() =>
            $"{(char)('A' + X)}{Y + 1}";
    }

    public enum CellState
    {
        Empty, // No ship, not shot
        Ship,  // Ship present, not shot
        Hit,   // Ship present, shot
        Miss   // No ship, shot
    }
}
