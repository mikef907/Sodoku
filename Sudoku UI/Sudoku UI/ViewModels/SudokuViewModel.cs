using Sudoku_Lib;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sudoku_UI.ViewModels
{
    public class SudokuViewModel : BaseViewModel
    {
        Sudoku sudoku = null;
        public bool showLabel { get => sudoku != null; }
        private string boardDisplay;
        private Grid sudokuGrid;

        public string BoardDisplay
        {
            get => boardDisplay;
            set => SetProperty(ref boardDisplay, value);
        }
        public SudokuViewModel(Grid sudokuGrid)
        {
            this.sudokuGrid = sudokuGrid;

            StartCommand = new Command(async() =>
            {
                sudoku = new Sudoku();
                await ShowBoard();
            });
        }

        public Command StartCommand { get; }

        public async Task ShowBoard()
        {
            sudokuGrid.Children.Clear();
            BoardDisplay = "Working...";

            await sudoku.Init();

            //string _boardDisplay = "";

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var stack = new FlexLayout()
                    {
                        BackgroundColor = new Color(0, 0, 0.5, 0.1),
                        Direction = FlexDirection.Column,
                        Wrap = FlexWrap.Wrap
                    };

                    var guesses = new Label 
                    { 
                        Text = string.Join(" ", sudoku.PossibleGuesses(i, j)), 
                        FontSize = 10,
                        IsVisible = !sudoku.PuzzleBoard[i, j].HasValue,
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions = LayoutOptions.Start
                    };

                    var label = new Entry
                    {
                        Text = sudoku.PuzzleBoard[i, j].ToString(),
                        IsReadOnly = sudoku.PuzzleBoard[i, j].HasValue,
                        FontSize = 18,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        VerticalOptions = LayoutOptions.Center,
                        Keyboard = Keyboard.Numeric,
                        MaxLength = 1,
                    };

                    stack.Children.Add(guesses);
                    stack.Children.Add(label);

                    sudokuGrid.Children.Add(stack, j, i);
                }
            }
            BoardDisplay = "";
        }
    }
}