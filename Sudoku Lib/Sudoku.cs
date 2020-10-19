using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sudoku_Lib
{
    public class Sudoku
    {
        private int?[,] GameBoard { get; set; }

        private Random Random { get; set; }

        public SudokuCellData[,] PuzzleBoard { get; private set; }

        public bool IsInit { get; private set; }

        public int Seed { get; private set; }

        public Sudoku()
        {
            IsInit = false;
            GameBoard = new int?[9, 9];
            PuzzleBoard = new SudokuCellData[9, 9];
        }

        public Sudoku(SudokuCellData[,] data, int seed) {
            IsInit = true;
            PuzzleBoard = data;
            Seed = seed;
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

        public void InitEmptyPuzzleBoard()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    PuzzleBoard[i, j] = new SudokuCellData(i, j, null);
        }

        private void InitPuzzleBoard()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
#if DEBUG
                    //PuzzleBoard[i, j] = new SudokuCellData(i, j, Random.Next() % 2 == 0 ? null : GameBoard[i, j]);

                    PuzzleBoard[i, j] = new SudokuCellData(i, j, GameBoard[i, j]);
            PuzzleBoard[0, 0].Value = null;
#else
            PuzzleBoard[i, j] = new SudokuCellData(i, j, Random.Next() % 2 == 0 ? null :  GameBoard[i, j]);
#endif
        }

        public int EmptyPuzzleBoardCellsCount()
        {
            int count = 0;

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (PuzzleBoard[i, j].Value == null)
                        count++;

            return count;
        }

        public int[] PossibleGuesses(int row, int col)
        {
            List<int> possible = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (int i = 0; i < 9; i++)
                possible.Remove(possible.FirstOrDefault(n => n == PuzzleBoard[row, i].Value));

            for (int i = 0; i < 9; i++)
                possible.Remove(possible.FirstOrDefault(n => n == PuzzleBoard[i, col].Value));

            int rowBoundary = row - (row % 3);
            int colBoundary = col - (col % 3);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    possible.Remove(possible.FirstOrDefault(n => n == PuzzleBoard[rowBoundary + i, colBoundary + j].Value));

            return possible.ToArray();
        }       
    }

    public class SudokuCellData: INotifyPropertyChanged
    {
        public readonly int Row;
        public readonly int Col;
        public bool UserInput { get; set; }
        public ObservableCollection<int> Data = new ObservableCollection<int>();

        private int? _value;
        public int? Value
        {
            get => _value;
            set {
                if (_value == value) return;
                _value = value;
                UserInput = true;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }

        public SudokuCellData(int row, int col, int? value)
        {
            Row = row;
            Col = col;
            _value = value;
            // UserInput = false;
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

        public static void ValidateRow(this SudokuCellData[,] board, int row, int? value)
        {
            for (int i = 0; i < 9; i++)
                if (board[row, i].Value == value)
                    throw new InvalidOperationException("Invalid Row");
        }

        public static void ValidateCol(this int?[,] board, int col, int? value)
        {
            for (int i = 0; i < 9; i++)
                if (board[i, col] == value)
                    throw new InvalidOperationException("Invalid Col");
        }

        public static void ValidateCol(this SudokuCellData[,] board, int col, int? value)
        {
            for (int i = 0; i < 9; i++)
                if (board[i, col].Value == value)
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

        public static void ValidateSquare(this SudokuCellData[,] board, int row, int col, int? value)
        {
            int rowBoundary = row - (row % 3);
            int colBoundary = col - (col % 3);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[rowBoundary + i, colBoundary + j].Value == value)
                        throw new InvalidOperationException("Invalid Square");
        }

        public static void SetCell(this int?[,] board, int row, int col, int? value)
        {
            if (value != null)
            {
                if (value < 1 || value > 9)
                    throw new ArgumentException("Invalid Value");
                try
                {
                    board.ValidateRow(row, value);
                    board.ValidateCol(col, value);
                    board.ValidateSquare(row, col, value);
                }
                catch (InvalidOperationException ex)
                {
                    throw ex;
                }
            }
            board[row, col] = value;
        }

        public static void SetCell(this SudokuCellData[,] board, int row, int col, int? value)
        {
            if (value != null)
            {
                if (value < 1 || value > 9)
                    throw new ArgumentException("Invalid Value");
                try
                {
                    board.ValidateRow(row, value);
                    board.ValidateCol(col, value);
                    board.ValidateSquare(row, col, value);
                }
                catch (InvalidOperationException ex)
                {
                    throw ex;
                }
            }
            board[row, col].Value = value;
        }

        public static bool IsSolved(this SudokuCellData[,] board)
        {
            bool isSolved;
            int?[,] solved = new int?[9, 9];
            try
            {
                for (int i = 0; i < 9; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        if (!board[i, j].Value.HasValue) return false;

                        solved.SetCell(i, j, board[i, j].Value);
                    }

                isSolved = true;
            }
            catch (ArgumentException)
            {
                isSolved = false;
            }
            catch (InvalidOperationException)
            {
                isSolved = false;
            }
            return isSolved;
        }

        public static bool IsComplete(this int?[,] board)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (!board[i, j].HasValue)
                        return false;
            return true;
        }

        public static bool IsComplete(this SudokuCellData[,] board)
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    if (!board[i, j].Value.HasValue)
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
