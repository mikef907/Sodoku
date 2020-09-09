using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Sodoku_Lib
{
    public class Sodoku
    {
        private int?[,] GameBoard { get; set; }
        public int?[,] PuzzleBoard { get; private set; }

        public Sodoku()
        {
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

        public async Task Init()
        {
            List<SodokuWorker> workers = new List<SodokuWorker>();
            SodokuWorker worker;

            await Task.Run(() =>
            {
                int iterations = 0;
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        iterations++;
                        if (this[i, j] == null)
                        {
                            worker = new SodokuWorker(i, j);
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

        private void InitPuzzleBoard()
        {
            var random = new Random();
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    PuzzleBoard[i, j] = random.Next() % 2 == 0 ? GameBoard[i, j] : null;
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

    public static class SodokuExt
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

    public class SodokuWorker
    {
        internal List<int> Left = new List<int>(9) { 1, 2, 3, 4, 5, 6, 7 ,8 ,9};
        internal int Row { get; }
        internal int Col { get; }
        internal int Current;

        public SodokuWorker(int row, int col)
        {
            Row = row;
            Col = col;
            SetNewCurrent();
        }

        public void SetNewCurrent()
        {
            if(Current != 0)
                Left.Remove(Current);

            if (Left.Count() > 0)
                Current = Left[new Random().Next(0, Left.Count())];
        }
    }
}
