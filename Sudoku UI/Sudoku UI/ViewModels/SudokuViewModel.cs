using Sudoku_Lib;
using Sudoku_UI.Models;
using Sudoku_UI.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Sudoku_UI.ViewModels
{
    public class SudokuViewModel : BaseViewModel
    {
        Sudoku sudoku = null;
        public bool showLabel { get => sudoku != null; }
        private TimeSpan span;
        private bool isInit;
        private Grid sudokuGrid;
        private SudokuPage page;
        private int seed;
        private GameTimer gameTimer { get; set; }

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

        public int Seed
        {
            get => seed;
            set => SetProperty(ref seed, value);
        }

        public SudokuViewModel(SudokuPage sudokuPage)
        {
            isInit = false;
            sudoku = new Sudoku();
            sudokuGrid = sudokuPage.grid;
            page = sudokuPage;

            page.ToolbarItems.Clear();

            StartCommand = new Command(async () => {
                await ShowBoard();
                page.ToolbarItems.Clear();
                page.ToolbarItems.Add(new ToolbarItem { Text = "Start Over", Command = StartOverCommand });
                page.ToolbarItems.Add(new ToolbarItem { Text = "Copy Seed", Command = CopySeedCommand });
            });

            StartOverCommand = new Command(async() =>
            {
                if (await page.DisplayAlert("Giving Up?", "Do you want to start over?", "Yes", "No"))
                {
                    sudoku = new Sudoku();
                    await ShowBoard();
                }
            });

            CopySeedCommand = new Command(async () =>
            {
                await Clipboard.SetTextAsync(Seed.ToString());
                await page.DisplayAlert("Seed Copied", $"Seed {Seed} copied to clipboard", "Gee, thanks");
            });

        }

        public Command StartOverCommand { get; }

        public Command StartCommand { get; }

        public Command CopySeedCommand { get; }

        public async Task InitBoard()
        {
            if (!string.IsNullOrEmpty(page.seedEntry.Text))
            {
                int seed;
                if (int.TryParse(page.seedEntry.Text, out seed))
                {
                    await sudoku.Init(seed);
                }
                else
                    await sudoku.Init();
            }
            else
                await sudoku.Init();
        }

        public async Task ShowBoard()
        {
            IsBusy = true;
            gameTimer?.Dispose();
            sudokuGrid.Children.Clear();

            await InitBoard();

            page.seedEntry.Text = null;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += TapGesture_Tapped;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var evenRow = i / 3 == 0 || i / 3 == 2;
                    var evenCol = j / 3 == 0 || j / 3 == 2 ;
                    var center = i / 3 == 1 && j / 3 == 1;
                    var accent = (evenRow && evenCol) || center ? 0.1 : 0.2;

                    string possibleGuesses = null;

                    if (!sudoku.PuzzleBoard[i, j].HasValue)
                    {
                        possibleGuesses = string.Join(" ", sudoku.PossibleGuesses(i, j));
                    }

                    var stack = new StackLayout()
                    {
                        BackgroundColor = new Color(0, 0, 0.5, accent),
                        BindingContext = $"{i}:{j}:{possibleGuesses}",
                        Spacing = 0
                    };

                    if (!sudoku.PuzzleBoard[i, j].HasValue) 
                    {
                        stack.GestureRecognizers.Add(tapGesture);
                    }

                    var guesses = new Label 
                    { 
                        Text = possibleGuesses, 
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)),
                        HeightRequest = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HorizontalOptions= LayoutOptions.FillAndExpand,
                    };

                    var entry = new Label()
                    {
                        Text = sudoku.PuzzleBoard[i, j].ToString(),
                        HorizontalTextAlignment = TextAlignment.Center,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
                    };

                    if (sudoku.PuzzleBoard[i, j].HasValue)
                        entry.FontAttributes = FontAttributes.Bold;

                    stack.Children.Add(guesses);
                    stack.Children.Add(entry);

                    sudokuGrid.Children.Add(stack, j, i);
                }
            }
            Seed = sudoku.Seed;
            IsInit = sudoku.IsInit;
            IsBusy = false;

            gameTimer = new GameTimer(SetGameTime());
            gameTimer.StartTimer();
        }

        private async void TapGesture_Tapped(object sender, EventArgs e)
        {
            var stackLayout = sender as StackLayout;
            var context = stackLayout.BindingContext.ToString().Split(':');
            string result = await page.DisplayPromptAsync("Input a number", $"Possible values are {context[2].Replace(" ",", ")}", maxLength: 1, keyboard: Keyboard.Numeric);
            if (await InputValue(result, Convert.ToInt32(context[0]), Convert.ToInt32(context[1])))
                (stackLayout.Children[1] as Label).Text = result;

        }

        private async void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            var element = sender as Entry;
            await InputValue(e.NewTextValue, (int)element.AnchorX, (int)element.AnchorY);
            element.Unfocus();
        }

        private Action SetGameTime() 
        {
            Timer = TimeSpan.FromSeconds(0);
            return () => Timer = Timer.Add(TimeSpan.FromSeconds(1));
        }
        

        private async Task<bool> InputValue(string textValue, int row, int col)
        {
            int value;

            if (string.IsNullOrEmpty(textValue))
            {
                sudoku.PuzzleBoard[row, col] = null;
                return true;
            }
            else if (int.TryParse(textValue, out value))
            {
                sudoku.PuzzleBoard[row, col] = value;
                return true;
            }

            if (sudoku.PuzzleBoard.IsComplete())
            {
                if (sudoku.Equals(sudoku.PuzzleBoard))
                {
                    gameTimer.StopTimer();
                    await page.DisplayAlert("Completed!", $"Your time is {Timer}", "PEACE!");
                    await ShowBoard();
                }
                return true;
            }
            return false;
        }
    }
}