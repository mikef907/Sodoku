using Sudoku_Lib;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Sudoku_Lib.Tests
{
    public class SudokuTests
    {
        Sudoku sudoku;
        public SudokuTests()
        {
            sudoku = new Sudoku();
            sudoku.InitEmptyPuzzleBoard();
        }

        [Fact()]
        public void SudokuCompletedFlagShouldInitFalse()
        {
            Assert.False(sudoku.PuzzleBoard.IsComplete());
        }

        [Fact()]
        public void SudokuGameboardShouldHave81Elements()
        {
            Assert.Equal(81, sudoku.Size());
        }

        [Fact()]
        public void ShouldBeAbleToReadElement()
        {
            Assert.IsType<SudokuCellData>(sudoku.PuzzleBoard[0, 0]);
            Assert.Null(sudoku.PuzzleBoard[0, 0].Value);
        }

        [Theory()]
        [InlineData(0, 0, 1)]
        [InlineData(1, 0, 5)]
        [InlineData(0, 2, 2)]
        [InlineData(4, 5, 7)]
        [InlineData(6, 6, 6)]
        [InlineData(1, 8, 1)]
        [InlineData(3, 2, 9)]
        public void ShouldBeAbleToAddIntToGameboard(int row, int col, int value)
        {
            sudoku.PuzzleBoard[row, col] = new SudokuCellData(row, col, true, value);
            Assert.Equal(value, sudoku.PuzzleBoard[row, col].Value);
        }

        [Theory()]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void ShouldThrowErrorIfNumberAppearsMoreThanOnceInRow(int row)
        {
            sudoku.PuzzleBoard[row, 0].Value = 1;
            Assert.Throws<InvalidOperationException>(() => sudoku.PuzzleBoard.ValidateRow(row, 1));
        }

        [Theory()]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void ShoudlThrowErrorIfNumberAppearsMoreThanOnceInCol(int col)
        {
            sudoku.PuzzleBoard[0, col].Value = 1;
            Assert.Throws<InvalidOperationException>(() => sudoku.PuzzleBoard.ValidateCol(col, 1));
        }

        [Theory()]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(3, 3)]
        [InlineData(3, 4)]
        [InlineData(4, 3)]
        [InlineData(4, 4)]
        [InlineData(7, 7)]
        public void ShouldThrowErrorIfNumberAppearsMoreThanOnceInSquare(int row, int col)
        {
            sudoku.PuzzleBoard[row, col].Value = 1;
            Assert.Throws<InvalidOperationException>(() => sudoku.PuzzleBoard.ValidateSquare(row, col, 1));
        }

        [Fact()]
        public void ShoudlBeAbleToSetAnyToNull()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    sudoku.PuzzleBoard[i, j] = null;
                    Assert.Null(sudoku.PuzzleBoard[i, j]);
                }
        }

        [Theory()]
        [InlineData(0, 0, -1)]
        [InlineData(1, 0, 0)]
        [InlineData(0, 2, 200)]
        [InlineData(4, 5, 10)]
        [InlineData(6, 6, -100)]
        [InlineData(1, 8, 999)]
        [InlineData(3, 2, 11)]
        public void ShouldOnlyExceptOneThruNine(int row, int col, int value)
        {
            Assert.Throws<ArgumentException>(() => sudoku.PuzzleBoard.SetCell(row, col, value));
        }

        [Fact()]
        public void ShouldReturnTrueWhenCompleted()
        {
            int c = 1;
            int o = 2;
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    sudoku.PuzzleBoard[i, j] = new SudokuCellData(i, j, true, c++);
                    if (c > 9) c = 1;
                }
                c = c + 3;
                if (c > 9)
                    c = o++;
            }


            Assert.True(sudoku.PuzzleBoard.IsComplete());
        }

        [Fact()]
        public async Task CanGenerateItsOwnPuzzle()
        {
            await sudoku.Init();
            Assert.True(sudoku.GameboardIsInit());
        }

        [Fact()]
        public async Task ShouldHavePuzzleBoardWithEmptyCells()
        {
            await sudoku.Init();
            Assert.NotNull(sudoku.PuzzleBoard);
            Assert.True(sudoku.EmptyPuzzleBoardCellsCount() > 0);
        }

        [Fact()]
        public async Task CanListPossibleGuessesForEmptyCell()
        {
            await sudoku.Init();

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    if (sudoku.PuzzleBoard[i, j] == null)
                    {
                        int[] guesses = sudoku.PossibleGuesses(i, j);
                        Assert.NotNull(guesses);

                        foreach (var guess in guesses)
                        {
                            sudoku.PuzzleBoard.ValidateRow(i, guess);
                            sudoku.PuzzleBoard.ValidateCol(j, guess);
                            sudoku.PuzzleBoard.ValidateSquare(i, j, guess);
                        }
                    }
                }

        }

        [Fact()]
        public async Task EqualsShouldBeFalseOnInit()
        {
            await sudoku.Init();
            Assert.False(sudoku.Equals(sudoku.PuzzleBoard));
        }

        [Fact()]
        public async Task SeededGamesShouldBeIdentical()
        {
            int seed = new Random().Next();
            await sudoku.Init(seed);

            var _sudoku = new Sudoku();
            await _sudoku.Init(seed);

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    Assert.Equal(sudoku.PuzzleBoard[i, j].Value, _sudoku.PuzzleBoard[i, j].Value);

        }
    }
}