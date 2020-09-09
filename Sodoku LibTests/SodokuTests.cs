using System;
using System.Threading.Tasks;
using Xunit;

namespace Sodoku_Lib.Tests
{
    public class SodokuTests
    {
        Sodoku sodoku;
        public SodokuTests() => sodoku = new Sodoku();

        [Fact()]
        public void SodokuCompletedFlagShouldInitFalse()
        {
            Assert.False(sodoku.PuzzleBoard.IsComplete());
        }

        [Fact()]
        public void SodokuGameboardShouldHave81Elements()
        {
            Assert.Equal(81, sodoku.Size());
        }

        [Fact()]
        public void ShouldBeAbleToReadElement()
        {
            Assert.Null(sodoku.PuzzleBoard[0, 0]);
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
            sodoku.PuzzleBoard[row, col] = value;
            Assert.Equal(value, sodoku.PuzzleBoard[row, col]);
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
            sodoku.PuzzleBoard[row, 0] = 1;
            Assert.Throws<InvalidOperationException>(() => sodoku.PuzzleBoard.ValidateRow(row, 1));
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
            sodoku.PuzzleBoard[0, col] = 1;
            Assert.Throws<InvalidOperationException>(() => sodoku.PuzzleBoard.ValidateCol(col, 1));
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
        [InlineData(7,7)]
        public void ShouldThrowErrorIfNumberAppearsMoreThanOnceInSquare(int row, int col)
        {
            sodoku.PuzzleBoard[row, col] = 1;
            Assert.Throws<InvalidOperationException>(() => sodoku.PuzzleBoard.ValidateSquare(row, col, 1));
        }

        [Fact()]
        public void ShoudlBeAbleToSetAnyToNull()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                { 
                    sodoku.PuzzleBoard[i, j] = null;
                    Assert.Null(sodoku.PuzzleBoard[i, j]);
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
            Assert.Throws<ArgumentException>(() => sodoku.PuzzleBoard.SetCell(row, col, value));
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
                    sodoku.PuzzleBoard[i, j] = c++;
                    if (c > 9) c = 1;
                }
                c = c + 3;
                if (c > 9)
                    c = o++;
            }
                    

            Assert.True(sodoku.PuzzleBoard.IsComplete());
        }

        [Fact()]
        public async Task CanGenerateItsOwnPuzzle()
        {
            await sodoku.Init();
            Assert.True(sodoku.GameboardIsInit());   
        }

        [Fact()]
        public async Task ShouldHavePuzzleBoardWithEmptyCells()
        {
            await sodoku.Init();
            Assert.NotNull(sodoku.PuzzleBoard);
            Assert.True(sodoku.EmptyPuzzleBoardCellsCount() > 0);
        }

        [Fact()]
        public async Task CanListPossibleGuessesForEmptyCell()
        {
            await sodoku.Init();

            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    if (sodoku.PuzzleBoard[i, j] == null)
                    {
                        int[] guesses = sodoku.PossibleGuesses(i, j);
                        Assert.NotNull(guesses);

                        foreach (var guess in guesses)
                        {
                            sodoku.PuzzleBoard.ValidateRow(i, guess);
                            sodoku.PuzzleBoard.ValidateCol(j, guess);
                            sodoku.PuzzleBoard.ValidateSquare(i, j, guess);
                        }
                    }
                }

        }
    }
}