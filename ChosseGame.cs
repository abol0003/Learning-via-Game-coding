using System;

namespace Learning
{
    /// <summary>
    /// Entry point that lets the user choose which game to play.
    /// </summary>
    public class Program
    {
        static void Main()
        {
            Console.WriteLine("Select a game to play:");
            Console.WriteLine("1) Battleship");
            Console.WriteLine("2) Tic-Tac-Toe");
            Console.Write("Your choice: ");

            string? choice = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(choice))
            {
                Console.WriteLine("No choice entered. Exiting.");
            }
            else
            {
                switch (choice.Trim())
                {
                    case "1":
                        var battleship = new BattleshipGame();
                        battleship.Setup();
                        battleship.Play();
                        break;

                    case "2":
                        var ticTacToe = new TicTacToeGame();
                        ticTacToe.Play();
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Exiting.");
                        break;
                }
            }
        }
    }
}
