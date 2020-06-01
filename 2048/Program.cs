using System;
using static System.Console;

namespace _2048
{
    class Program
    {
        static void Main(string[] args)
        {
            Game startNewGame;
            int userInput = 0;
            
            Write("Enter 1 to continue or Enter 2 to start a new game: ");
            int.TryParse(ReadLine(), out userInput);
            if (userInput == 1)
            {
                startNewGame = new Game(false);
            }
            else
            {
                startNewGame = new Game(true);
            }
            startNewGame.Run();
        }
    }
}
