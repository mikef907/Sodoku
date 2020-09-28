using Sudoku_Lib;
using Sudoku_UI.Views;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sudoku_UI.ViewModels
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
    public class SudokuViewModel : BaseViewModel
    {
        Sudoku sudoku = null;
        public bool showLabel { get => sudoku != null; }
        private TimeSpan span;
        private bool isInit;
        private Grid sudokuGrid;
        private SudokuPage page;


        public bool IsInit
        {
            get => isInit;
            set => SetProperty(ref isInit, value);
        }

        public TimeSpan Timer
        {
            get => span;
            set => SetProperty(ref span, value);
        }

        public SudokuViewModel(SudokuPage sudokuPage)
        {
            isInit = false;
            sudoku = new Sudoku();
            this.sudokuGrid = sudokuPage.grid;
            page = sudokuPage;

            StartCommand = new Command(async () => await ShowBoard());

            StartOverCommand = new Command(async() =>
            {
                if (await page.DisplayAlert("Giving Up?", "Do you want to start over?", "Yes", "No"))
                {
                    sudoku = new Sudoku();
                    await ShowBoard();
                }
            });

        }

        public Command StartOverCommand { get; }

        public Command StartCommand { get; }

        public async Task ShowBoard()
        {
            IsBusy = true;
            sudokuGrid.Children.Clear();
            await sudoku.Init();


            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var evenRow = i / 3 == 0 || i / 3 == 2;
                    var evenCol = j / 3 == 0 || j / 3 == 2 ;
                    var center = i / 3 == 1 && j / 3 == 1;
                    var accent = (evenRow && evenCol) || center ? 0.1 : 0.2;

                    var stack = new StackLayout()
                    {
                        BackgroundColor = new Color(0, 0, 0.5, accent),
                        Orientation = StackOrientation.Horizontal
                    };

                    var guesses = new Label 
                    { 
                        Text = sudoku.PuzzleBoard[i, j].HasValue ? null : string.Join(" ", sudoku.PossibleGuesses(i, j)), 
                        FontSize = 10
                    };


                    var entry = new Entry
                    {
                        Text = sudoku.PuzzleBoard[i, j].ToString(),
                        IsReadOnly = sudoku.PuzzleBoard[i, j].HasValue,
                        FontSize = 16,
                        Keyboard = Keyboard.Numeric,
                        MaxLength = 1,
                        AnchorX = i,
                        AnchorY = j
                    };


                    entry.TextChanged += Entry_TextChanged;

                    stack.Children.Add(guesses);
                    stack.Children.Add(entry);

                    sudokuGrid.Children.Add(stack, j, i);
                }
            }
            IsInit = sudoku.IsInit;

            Timer = TimeSpan.FromSeconds(0);
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                Timer = Timer.Add(TimeSpan.FromSeconds(1));
                return true;
            });

            IsBusy = false;
        }

        private async void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var element = sender as Entry;
            await InputValue(e.NewTextValue, (int)element.AnchorX, (int)element.AnchorY);
            element.Unfocus();
        }

        private async Task InputValue(string textValue, int row, int col)
        {
            int value;

            if (string.IsNullOrEmpty(textValue))
            {
                sudoku.PuzzleBoard[row, col] = null;
            }
            else if (int.TryParse(textValue, out value))
            {
                sudoku.PuzzleBoard[row, col] = value;
            }

            if (sudoku.PuzzleBoard.IsComplete())
            {
                if (sudoku.Equals(sudoku.PuzzleBoard))
                {
                    await page.DisplayAlert("Complete!", "We're done here!", "PEACE!");
                    await ShowBoard();
                }
            }
        }
    }
}