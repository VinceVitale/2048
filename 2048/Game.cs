using System;
using System.IO;
using System.Collections.Generic;
using static System.ConsoleColor;
using static System.Console;
//using static System.IO.FileMode;
//using static System.ConsoleKey;
//using static System.Math;
//using static _2048.Game.Direction;

namespace _2048
{
    class Game
    {
        public int HighScore { get; set; }
        public int UserScore { get; set; }
        public int HighTile { get; set; }
        public int BoardSize { get; }
        public int[,] GameBoard { get; }
        public int boardRows { get; }
        public int boardColumns { get; }
        public int NumberOfMoves { get; set; }
        public bool NewGame { get; }
        Random randomNumb = new Random();
        public enum Direction { Up, Down, Right, Left }
        string fileInfo = @"G:\2048Programs\2048C#\My2048C#\2048Console\My2048Console\2048\SaveFolder\SaveFile.txt";
        
        public Game(bool newGame)
        {
            int fileScore = 0, fileTile = 2;
            GetFileInfo(ref fileScore, ref fileTile);

            HighScore = fileScore;
            UserScore = 0;
            HighTile = fileTile;
            BoardSize = 4;
            GameBoard = new int[BoardSize, BoardSize];
            boardRows = GameBoard.GetLength(0);
            boardColumns = GameBoard.GetLength(1);
            NumberOfMoves = -1;
            NewGame = newGame;
        }
        
        public void Run()
        {
            bool hasUpdated = true;
            if (NewGame == false)
            {
                GameBoard[0, 0] = HighTile;
            }
            else
            {
                GenerateRandomNumber();
            }
            while (true)
            {
                if (hasUpdated)
                {
                    ++NumberOfMoves;
                    GenerateRandomNumber();
                }

                DisplayBoard();

                if (GameOver())
                {
                    const string DELIM = ",";
                    FileStream fileInfo2 = new FileStream(fileInfo, FileMode.Create, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fileInfo2);
                    sw.WriteLine(HighScore + DELIM + HighTile);
                    sw.Close();
                    fileInfo2.Close();
                    break;
                }
                ForegroundColor = White;
                WriteLine("Use the arrow keys to move. (Press Ctrl-C to exit)");
                ConsoleKeyInfo input = ReadKey(true);

                switch (input.Key)
                {
                    case ConsoleKey.UpArrow:
                        hasUpdated = Update(Direction.Up);
                        break;

                    case ConsoleKey.DownArrow:
                        hasUpdated = Update(Direction.Down);
                        break;

                    case ConsoleKey.LeftArrow:
                        hasUpdated = Update(Direction.Left);
                        break;

                    case ConsoleKey.RightArrow:
                        hasUpdated = Update(Direction.Right);
                        break;

                    default:
                        hasUpdated = false;
                        break;
                }
            }// use CTRL-C to break out of while loop

            if (UserScore == HighScore)
            {
                ForegroundColor = Green;
                WriteLine("New High Score!!!");
            }
            else
            {
                ForegroundColor = Red;
                WriteLine("Game Over...\n\nG_G");
            }
            Read();
        }

        private void DisplayBoard()
        {
            Clear();
            BackgroundColor = Black;
            if (NewGame == true)
            {
                ForegroundColor = Yellow;
                WriteLine("\n  2048\n");
            }
            else
            {
                ForegroundColor = Green;
                WriteLine("\n  {0}\n", HighTile);
            }
            for (int i = 0; i < boardRows; i++)
            {
                for (int j = 0; j < boardColumns; j++)
                {
                    ForegroundColor = NumberColors(GameBoard[i, j]);
                    Write(string.Format("{0,6}", GameBoard[i, j]));

                    if (GameBoard[i, j] > HighTile)
                    {
                        HighTile = GameBoard[i, j];
                    }
                }
                WriteLine("\n\n");
            }

            if (UserScore >= HighScore)
            {
                HighScore = UserScore;
                ForegroundColor = Red;
            }
            else
            {
                ForegroundColor = Cyan;
            }
            WriteLine("  High Score: {0}\n", HighScore);
            ForegroundColor = Green;
            WriteLine("  Score: {0}\n", UserScore);
            ForegroundColor = Yellow;
            WriteLine("  Moves: {0}\n", NumberOfMoves);
        }

        private bool Update(Direction direction)
        {
            int addedPoints;
            bool isUpdated = Update(GameBoard, direction, out addedPoints);
            UserScore += addedPoints;
            return isUpdated;
        }
        
        private static bool Update(int[,] gBoard, Direction direction, out int addedPoints)
        {
            int boardRows = gBoard.GetLength(0);
            int boardColumns = gBoard.GetLength(1);
            bool hasUpdated = false;
            addedPoints = 0;

            // Drop along row or column? true: process inner along row; false: process inner along column
            bool isAlongRow = direction == Direction.Left || direction == Direction.Right;

            // Should we process inner dimension in increasing index order?
            bool isIncreasing = direction == Direction.Left || direction == Direction.Up;

            int outterCount = isAlongRow ? boardRows : boardColumns;
            int innerCount = isAlongRow ? boardColumns : boardRows;
            int innerStart = isIncreasing ? 0 : innerCount - 1;
            int innerEnd = isIncreasing ? innerCount - 1 : 0;

            Func<int, int> drop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex - 1)
                : new Func<int, int>(innerIndex => innerIndex + 1);

            Func<int, int> reverseDrop = isIncreasing
                ? new Func<int, int>(innerIndex => innerIndex + 1)
                : new Func<int, int>(innerIndex => innerIndex - 1);

            Func<int[,], int, int, int> getValue = isAlongRow
                ? new Func<int[,], int, int, int>((x, i, j) => x[i, j])
                : new Func<int[,], int, int, int>((x, i, j) => x[j, i]);

            Action<int[,], int, int, int> setValue = isAlongRow
                ? new Action<int[,], int, int, int>((x, i, j, v) => x[i, j] = v)
                : new Action<int[,], int, int, int>((x, i, j, v) => x[j, i] = v);

            Func<int, bool> innerCondition = index => Math.Min(innerStart, innerEnd) <= index && index <= Math.Max(innerStart, innerEnd);

            for (int i = 0; i < outterCount; i++)
            {
                for (int j = innerStart; innerCondition(j); j = reverseDrop(j))
                {
                    if (getValue(gBoard, i, j) == 0)
                    {
                        continue;
                    }
                    int newJ = j;
                    do
                    {
                        newJ = drop(newJ);
                    }
                    while (innerCondition(newJ) && getValue(gBoard, i, newJ) == 0);

                    if (innerCondition(newJ) && getValue(gBoard, i, newJ) == getValue(gBoard, i, j))
                    {
                        int newValue = getValue(gBoard, i, newJ) * 2;
                        setValue(gBoard, i, newJ, newValue);
                        setValue(gBoard, i, j, 0);
                        hasUpdated = true;
                        addedPoints += newValue;
                    }
                    else
                    {
                        newJ = reverseDrop(newJ);
                        if (newJ != j)
                        {
                            hasUpdated = true;
                        }
                        int value = getValue(gBoard, i, j);
                        setValue(gBoard, i, j, 0);
                        setValue(gBoard, i, newJ, value);
                    }
                }
            }
            return hasUpdated;
        }

        private void GenerateRandomNumber()
        {
            List<Tuple<int, int>> emptyBoardSlots = new List<Tuple<int, int>>();
            for (int iRow = 0; iRow < boardRows; iRow++)
            {
                for (int iCol = 0; iCol < boardColumns; iCol++)
                {
                    if (GameBoard[iRow, iCol] == 0)
                    {
                        emptyBoardSlots.Add(new Tuple<int, int>(iRow, iCol));
                    }
                }
            }
            int iSlot = this.randomNumb.Next(0, emptyBoardSlots.Count);
            int randomNumb = this.randomNumb.Next(0, 100) < 90 ? (int)2 : (int)4;
            GameBoard[emptyBoardSlots[iSlot].Item1, emptyBoardSlots[iSlot].Item2] = randomNumb;
        }

        private static ConsoleColor NumberColors(int tileNumber)
        {
            switch (tileNumber)
            {
                case 0:
                    return DarkGray;
                case 2:
                    return White;
                case 4:
                    return Green;
                case 8:
                    return DarkGreen;
                case 16:
                    return Cyan;
                case 32:
                    return DarkCyan;
                case 64:
                    return Blue;
                case 128:
                    return DarkBlue;
                case 256:
                    return Magenta;
                case 512:
                    return DarkMagenta;
                case 1024:
                    return DarkYellow;
                case 2048:
                    return Yellow;
                case 4096:
                    return Red;
                case 8192:
                    return DarkRed;
                default:
                    return DarkRed;
            }
        }

        private bool GameOver()
        {
            int addedPoints;
            foreach (Direction arrowKeyDirection in new Direction[] { Direction.Down, Direction.Up, Direction.Left, Direction.Right })
            {
                int[,] boardClone = (int[,])GameBoard.Clone();
                if (Update(boardClone, arrowKeyDirection, out addedPoints))
                {
                    return false;
                }
            }
            return true;
        }

        private void GetFileInfo(ref int fileScore, ref int fileTile)
        {
            const char DELIM = ',';
            string recordIn;
            string[] fields;

            if (File.Exists(fileInfo))
            {
                FileStream fileInfo1 = new FileStream(fileInfo, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fileInfo1);
                
                fileInfo1.Seek(0, SeekOrigin.Begin);
                recordIn = sr.ReadLine();
                fields = recordIn.Split(DELIM);
                int.TryParse(fields[0], out fileScore);
                int.TryParse(fields[1], out fileTile);

                sr.Close();
                fileInfo1.Close();
            }
            else
            {
                fileScore = 0;
                fileTile = 2;
            }
        }
    }
}
