using Sudoku_Lib;
using Sudoku_UI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Sudoku_UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SudokuPage : ContentPage
    {
        Sudoku sudoku;
        Color gridColor = Color.FromHex("#894357");
        private GameTimer _gameTimer;
        private AbsoluteLayout _selectedCell;
        private TimeSpan _timer;
        private int _seed;
        private bool _showWorkBench;

        public TimeSpan Timer { 
            get => _timer; 
            set {
                _timer = value;
                OnPropertyChanged();
            } 
        }

        public int Seed {
            get => _seed;
            set {
                _seed = value;
                OnPropertyChanged();
            }
        }

        public bool ShowWorkbench { 
            get => _showWorkBench;
            set {
                _showWorkBench = value;
                OnPropertyChanged();
            }
        }

        public Command StartOverCommand { get; }
        public Command CopySeedCommand { get; }

        public SudokuPage()
        {
            InitializeComponent();
            BindingContext = this;
            ShowWorkbench = false;
            gameStack.IsVisible = false;
            gameStack.HeightRequest = 0;
            ToolbarItems.Clear();

            StartOverCommand = new Command(async () =>
            {
                if (await DisplayAlert("Giving Up?", "Do you want to start over?", "Yes", "No"))
                {
                    await ShowBoard();
                }
            });

            CopySeedCommand = new Command(async () =>
            {
                await Clipboard.SetTextAsync(Seed.ToString());
                await DisplayAlert("Seed Copied", $"Seed {Seed} copied to clipboard", "Gee, thanks");
            });
        }

        public async Task InitBoard()
        {
            sudoku = new Sudoku();
            if (!string.IsNullOrEmpty(seedEntry.Text))
            {
                int seed;
                if (int.TryParse(seedEntry.Text, out seed))
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
            _gameTimer?.Dispose();
            grid.Children.Clear();
            _selectedCell = null;
            ShowWorkbench = false;

            await InitBoard();

            seedEntry.Text = null;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += TapGesture_Tapped;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var stack = new AbsoluteLayout()
                    {
                        BackgroundColor = new Color(gridColor.R, gridColor.G, gridColor.B, GetAccent(i, j)),
                        BindingContext = new SudokuCellData(i, j)
                    };

                    if (!sudoku.PuzzleBoard[i, j].HasValue)
                    {
                        stack.GestureRecognizers.Add(tapGesture);
                    }

                    var guesses = new Label
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) / 1.4,
                        HeightRequest = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 1.8,
                        LineBreakMode = LineBreakMode.CharacterWrap
                    };

                    var entry = new Label()
                    {
                        Text = sudoku.PuzzleBoard[i, j].ToString(),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalOptions = LayoutOptions.End,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
                    };


                    if (sudoku.PuzzleBoard[i, j].HasValue)
                        entry.FontAttributes = FontAttributes.Bold;

                    stack.Children.Add(guesses, new Rectangle(0, 0, 1, 0.5), AbsoluteLayoutFlags.All);
                    stack.Children.Add(entry, new Rectangle(0, 0.5, 1, 1), AbsoluteLayoutFlags.All);

                    grid.Children.Add(stack, j, i);
                }
            }
            Seed = sudoku.Seed;
            _gameTimer = new GameTimer(SetGameTime());
            _gameTimer.StartTimer();

            startStack.IsVisible = false;
            startStack.HeightRequest = 0;

            gameStack.IsVisible = true;
            gameStack.HeightRequest = Device.GetNamedSize(NamedSize.Default, typeof(StackLayout));

            IsBusy = false;
        }

        private void TapGesture_Tapped(object sender, EventArgs e)
        {
            if (_selectedCell != null)
            {
                var _selectedContext = _selectedCell.BindingContext as SudokuCellData;
                _selectedCell.BackgroundColor = new Color(gridColor.R, gridColor.G, gridColor.B, GetAccent(_selectedContext.Row, _selectedContext.Col));
            }

            _selectedCell = sender as AbsoluteLayout;
            _selectedCell.BackgroundColor = Color.Yellow;

            var context = _selectedCell.BindingContext as SudokuCellData;

            numberStrip.Children.ForEach(child =>
            {
                var btn = child as Button;
                var number = Convert.ToInt32(btn.Text);
                if (context.Data.Contains(number))
                {
                    btn.TextColor = Color.DarkGreen;
                }
                else
                {
                    btn.TextColor = Color.LightGray;
                }
            });

            valueEntry.Text = sudoku.PuzzleBoard[context.Row, context.Col]?.ToString();
            ShowWorkbench = true;
        }

        private double GetAccent(int row, int col)
        {
            var evenRow = row / 3 == 0 || row / 3 == 2;
            var evenCol = col / 3 == 0 || col / 3 == 2;
            var center = row / 3 == 1 && col / 3 == 1;
            return (evenRow && evenCol) || center ? 0.1 : 0.2;
        }

        private Action SetGameTime()
        {
            Timer = TimeSpan.FromSeconds(0);
            return () => Timer = Timer.Add(TimeSpan.FromSeconds(1));
        }


        private bool InputValue(string textValue, int row, int col)
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
            return false;
        }

        private async Task CheckBoardState()
        {
            if (sudoku.PuzzleBoard.IsComplete())
            {
                if (sudoku.Equals(sudoku.PuzzleBoard))
                {
                    _gameTimer.StopTimer();
                    await DisplayAlert("Completed!", $"Your time is {Timer}", "PEACE!");
                    await ShowBoard();
                }
                else
                {
                    await DisplayAlert("Oh No!", "Something's not right...", "Got it");
                }
            }
        }

        private async void StartGame(object sender, EventArgs e)
        {
            await ShowBoard();
            ToolbarItems.Clear();
            ToolbarItems.Add(new ToolbarItem { Text = "Start Over", Command = StartOverCommand });
            ToolbarItems.Add(new ToolbarItem { Text = "Copy Seed", Command = CopySeedCommand });
        }

        private void ToggleValue(object sender, EventArgs e)
        {
            var btn = sender as Button;
            var label = (_selectedCell.Children[0] as Label);
            var values = _selectedCell.BindingContext as SudokuCellData;
            var number = Convert.ToInt32(btn.Text);
            if (values.Data.Contains(number))
            {
                values.Data.Remove(number);
                btn.TextColor = Color.LightGray;
            }
            else
            {
                values.Data.Add(number);
                btn.TextColor = Color.DarkGreen;
            }
            values.Data.Sort();
            label.Text = string.Join(" ", values.Data);
        }

        private async void EntryCell_Completed(object sender, EventArgs e)
        {
            var data = _selectedCell.BindingContext as SudokuCellData;
            var entryCell = sender as Entry;
            var value = entryCell.Text;
            if (InputValue(value, data.Row, data.Col))
            {
                (_selectedCell.Children[1] as Label).Text = value;
                entryCell.Unfocus();
                await CheckBoardState();
            }
            entryCell.Unfocus();
        }
    }
}