using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Sudoku_Lib
{
    public class Sudoku
    {
        private int?[,] GameBoard { get; set; }

        private Random Random { get; set; }

        public int?[,] PuzzleBoard { get; private set; }

        public bool IsInit { get; private set; }

        public int Seed { get; private set; }

        public Sudoku()
        {
            IsInit = false;
            GameBoard = new int?[9, 9];
            PuzzleBoard = new int?[9, 9];
        }

        private int? this[int row, int col]
        {
            get => GameBoard[row, col];
            set => GameBoard.SetCell(row, col, value);
        }

        public int Size() => GameBoard.Length;

        public bool GameboardIsInit() => GameBoard.IsComplete();

        public async Task Init(int? seed = null)
        {
            List<SudokuWorker> workers = new List<SudokuWorker>();
            SudokuWorker worker;

            SetSeed(seed);

            await Task.Run(() =>
            {
                int iterations = 0;
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        iterations++;
                        if (this[i, j] == null)
                        {
                            worker = new SudokuWorker(i, j, Random);
                            workers.Add(worker);
                        }
                        else
                        {
                            worker = workers.Single(w => w.Row == i && w.Col == j);
                            worker.SetNewCurrent();
                        }

                        while (worker.Left.Count > 0)
                        {
                            try
                            {
                                this[i, j] = worker.Current;
                                break;
                            }
                            catch (InvalidOperationException)
                            {
                                worker.SetNewCurrent();
                            }
                        }

                        if (worker.Left.Count == 0)
                        {
                            this[i, j] = null;
                            workers.Remove(worker);
                            if (j > 0) j -= 2;
                            else
                            {
                                j = 7;
                                i--;
                            }
                        }

                    }
                InitPuzzleBoard();
                IsInit = true;

#if DEBUG
                Debug.WriteLine($"Iterations:{iterations}");
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                        Debug.Write($"| {this[i, j]} ");
                    Debug.WriteLine("|");
                }

#endif
            });
        }

        private void SetSeed(int? seed = null) {
            Seed = seed.HasValue ? seed.Value : new Random().Next();
            Random = new Random(Seed);
        }

        public bool Equals(int?[,] gameboard)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (GameBoard[i, j] != gameboard[i, j])
                        return false;
            return true;
        }

        private void InitPuzzleBoard()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    PuzzleBoard[i, j] = Random.Next() % 2 == 0 ? null :  GameBoard[i, j];
        }

        public int EmptyPuzzleBoardCellsCount()
        {
            int count = 0;

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (PuzzleBoard[i, j] == null)
                        count++;

            return count;
        }

        public int[] PossibleGuesses(int row, int col)
        {
            List<int> possible = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int i = 0; i < 9; i++)
                possible.Remove(possible.FirstOrDefault(n => n == PuzzleBoard[row, i]));

            for (int i = 0; i < 9; i++)
                possible.Remove(possible.FirstOrDefault(n => n == PuzzleBoard[i, col]));

            int rowBoundary = row - (row % 3);
            int colBoundary = col - (col % 3);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    possible.Remove(possible.FirstOrDefault(n => n == PuzzleBoard[rowBoundary + i, colBoundary + j]));

            return possible.ToArray();
        }       
    }

    public static class SudokuExt
    {
        public static void ValidateRow(this int?[,] board, int row, int? value)
        { 
            for (int i = 0; i< 9; i++)
                if (board[row, i] == value)
                    throw new InvalidOperationException("Invalid Row");        
        }

        public static void ValidateCol(this int?[,] board, int col, int? value)
        {
            for (int i = 0; i < 9; i++)
                if (board[i, col] == value)
                    throw new InvalidOperationException("Invalid Col");
        }

        public static void ValidateSquare(this int?[,] board, int row, int col, int? value)
        {
            int rowBoundary = row - (row % 3);
            int colBoundary = col - (col % 3);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[rowBoundary + i, colBoundary + j] == value)
                        throw new InvalidOperationException("Invalid Square");
        }

        public static void SetCell(this int?[,] board, int row, int col, int? value)
        {
            if (value != null)
            {
                if (value < 1 || value > 9)
                    throw new ArgumentException("Invalid Value");
                board.ValidateRow(row, value);
                board.ValidateCol(col, value);
                board.ValidateSquare(row, col, value);
            }
            board[row, col] = value;
        }

        public static bool IsComplete(this int?[,] board)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] == null)
                        return false;
            return true;
        }
    }

    public class SudokuWorker
    {
        internal List<int> Left = new List<int>(9) { 1, 2, 3, 4, 5, 6, 7 ,8 ,9};
        internal int Row { get; }
        internal int Col { get; }
        internal int Current;
        private Random random;

        public SudokuWorker(int row, int col, Random random)
        {
            Row = row;
            Col = col;
            this.random = random;
            SetNewCurrent();
        }

        public void SetNewCurrent()
        {
            if(Current != 0)
                Left.Remove(Current);

            if (Left.Count() > 0)
                Current = Left[random.Next(0, Left.Count())];
        }
    }
}
