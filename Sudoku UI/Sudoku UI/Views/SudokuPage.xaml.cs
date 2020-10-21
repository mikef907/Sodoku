using Newtonsoft.Json;
using SQLite;
using Sudoku_Lib;
using Sudoku_UI.Models;
using SudokuUI.Persistence;
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
        private readonly Color gridColor = Color.FromHex("#894357");

        private readonly Color bgAccentA;
        private readonly Color bgAccentB;

        private GameTimer _gameTimer;
        private AbsoluteLayout _selectedCell;
        private TimeSpan _timer;
        private int _seed;
        private bool _showWorkBench;
        private SQLiteAsyncConnection _db;

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
            DeviceDisplay.KeepScreenOn = true;
            BindingContext = this;
            _db = DependencyService.Get<ISQLiteDb>().GetConnection();

            bgAccentA = new Color(gridColor.R, gridColor.G, gridColor.B, 0.1);
            bgAccentB = new Color(gridColor.R, gridColor.G, gridColor.B, 0.2);

            StartOverCommand = new Command(async () =>
            {
                var seed = await DisplayPromptAsync("Start Over?", 
                    "Enter a seed value to generate a new puzzle.", 
                    initialValue: new Random().Next().ToString());

                if (!string.IsNullOrEmpty(seed))
                {
                    await InsertGameAsync();
                    int _seed;
                    if (int.TryParse(seed, out _seed))
                        await ShowBoard(_seed);
                    else
                        await ShowBoard();
                }
            });

            CopySeedCommand = new Command(async () =>
            {
                await Clipboard.SetTextAsync(Seed.ToString());
                await DisplayAlert("Seed Copied", $"Seed {Seed} copied to clipboard", "Gee, thanks");
            });

        }

        protected override async void OnAppearing()
        {
            IsBusy = true;
            ShowWorkbench = false;

            gameStack.IsVisible = false;
            gameStack.HeightRequest = 0;

            startStack.IsVisible = false;

            ToolbarItems.Clear();

            seedEntry.Text = new Random().Next().ToString();

            if (!await Deserialize())
            {
                startStack.IsVisible = true;
            }

            InitGameTimer(reset: false);

            IsBusy = false;
            base.OnAppearing();
        }

        protected override async void OnDisappearing()
        {
            _gameTimer?.StopTimer();
            _gameTimer = null;

            if (sudoku != null)
            {
                var json = JsonConvert.SerializeObject(sudoku.PuzzleBoard);

                var current = new CurrentGame
                {
                    State = json,
                    Seed = sudoku.Seed,
                    Timer = Timer
                };

                await _db.DeleteAllAsync<CurrentGame>();
                await _db.InsertOrReplaceAsync(current);
            }
            base.OnDisappearing();
        }

        private async Task<bool> Deserialize() {
            var current = await _db.Table<CurrentGame>().FirstOrDefaultAsync();

            if (current != null)
            {
                SudokuCellData[,] state = JsonConvert.DeserializeObject<SudokuCellData[,]>(current.State);
                sudoku = new Sudoku(state, current.Seed);
                Timer = current.Timer;
                InitGrid();
                InitGameState();
                InitGameTimer(reset: false);
                InitToolBar();
                return true;
            }
            return false;
        }

        private async Task InsertGameAsync() {

            var game = new SudokuGame
            {
                Time = Timer,
                Seed = sudoku.Seed,
                Solved = sudoku.PuzzleBoard.IsSolved(),
                Attempt = await _db.Table<SudokuGame>().Where(s => s.Seed == sudoku.Seed).CountAsync() + 1
            };

            await _db.InsertAsync(game);
        }

        public async Task InitBoard(int? seed = null)
        {
            sudoku = new Sudoku();
            await sudoku.Init(seed);
        }

        public void InitGrid()
        {
            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += TapGesture_Tapped;

            for (int i = 0; i < 9; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = Device.GetNamedSize(NamedSize.Default, typeof(Label)) * 2.7 });
                for (int j = 0; j < 9; j++)
                {
                    var stack = new AbsoluteLayout()
                    {
                        BackgroundColor = GetBGAccent(i, j),
                        BindingContext = sudoku.PuzzleBoard[i, j]
                    };

                    var guesses = new Label
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 1.1,
                        Text = string.Join(" ", sudoku.PuzzleBoard[i, j].Data),
                        VerticalOptions = LayoutOptions.Start
                    };

                    var entry = new Label()
                    {
                        Text = sudoku.PuzzleBoard[i, j].Value.ToString(),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalOptions = LayoutOptions.End,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
                    };

                    stack.GestureRecognizers.Add(tapGesture);

                    if (sudoku.PuzzleBoard[i, j].Editable)
                        entry.TextColor = gridColor;
                    else
                        entry.FontAttributes = FontAttributes.Bold;

                    stack.Children.Add(guesses, new Rectangle(0, 0, 1, 0.4), AbsoluteLayoutFlags.All);
                    stack.Children.Add(entry, new Rectangle(0, 0.5, 1, 1), AbsoluteLayoutFlags.All);

                    grid.Children.Add(stack, j, i);
                   
                }
            }
        }

        private void InitGameState()
        {
            seedEntry.Text = null;
            _selectedCell = null;
            ShowWorkbench = false;
            Seed = sudoku.Seed;

            startStack.IsVisible = false;
            startStack.HeightRequest = 0;

            gameStack.IsVisible = true;
            gameStack.HeightRequest = Device.GetNamedSize(NamedSize.Default, typeof(StackLayout));
        }

        private void InitGameTimer(bool reset = false)
        {
            _gameTimer?.Dispose();
            _gameTimer = new GameTimer(SetGameTime(reset));
            _gameTimer.InitTimer();
            _gameTimer.StartTimer();
        }

        public void InitToolBar()
        {
            ToolbarItems.Clear();
            ToolbarItems.Add(new ToolbarItem { Text = "Start Over", Command = StartOverCommand });
            ToolbarItems.Add(new ToolbarItem { Text = "Copy Seed", Command = CopySeedCommand });
        }

        private async Task ShowBoard(int? seed = null)
        {
            IsBusy = true;
            
            await InitBoard(seed);

            InitGrid();

            InitGameState();

            InitGameTimer(reset: true);

            IsBusy = false;
        }

        private void TapGesture_Tapped(object sender, EventArgs e)
        {
            if (_selectedCell != null)
            {
                var _selectedContext = _selectedCell.BindingContext as SudokuCellData;
                grid.Children.ForEach(c => c.BackgroundColor = GetBGAccent(Grid.GetColumn(c), Grid.GetRow(c)));
            }

            _selectedCell = sender as AbsoluteLayout;

            var context = _selectedCell.BindingContext as SudokuCellData;

            grid.Children.Where(c => Grid.GetColumn(c) == context.Col).ForEach(c => c.BackgroundColor = Color.LightSalmon);
            grid.Children.Where(c => Grid.GetRow(c) == context.Row).ForEach(c => c.BackgroundColor = Color.LightSalmon);

            if (context.Value.HasValue)
                HighlightSameNumber(context);

            _selectedCell.BackgroundColor = Color.Yellow;

            if (context.Editable)
            {
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

                valueEntry.Text = sudoku.PuzzleBoard[context.Row, context.Col].Value?.ToString();
                ShowWorkbench = true;
            }
            else ShowWorkbench = false;


        }

        private Color GetBGAccent(int row, int col)
        {
            var evenRow = row / 3 == 0 || row / 3 == 2;
            var evenCol = col / 3 == 0 || col / 3 == 2;
            var center = row / 3 == 1 && col / 3 == 1;
            return (evenRow && evenCol) || center ? bgAccentA : bgAccentB;
        }

        private Action SetGameTime(bool reset = false)
        {
            if(reset)
                Timer = TimeSpan.FromSeconds(0);

            return () => Timer = Timer.Add(TimeSpan.FromSeconds(1));
        }

        private bool InputValue(string textValue, int row, int col)
        {
            int value;

            if (string.IsNullOrEmpty(textValue))
            {
                sudoku.PuzzleBoard[row, col].Value = null;
                return true;
            }
            else if (int.TryParse(textValue, out value))
            {
                if (value > 0)
                { 
                    sudoku.PuzzleBoard[row, col].Value = value;
                    return true;             
                }
            }
            return false;
        }

        private async Task CheckBoardState()
        {
            if (sudoku.PuzzleBoard.IsComplete())
            {
                if (sudoku.PuzzleBoard.IsSolved())
                {
                    _gameTimer.StopTimer();
                    await DisplayAlert("Completed!", $"Your time for the seed {Seed} is {Timer}", "Start Over");
                    await InsertGameAsync();
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
            int seed;

            if (int.TryParse(seedEntry.Text, out seed))
                await ShowBoard(seed);
            else
                await ShowBoard();

            InitToolBar();
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
            var context = _selectedCell.BindingContext as SudokuCellData;
            var entryCell = sender as Entry;
            var value = entryCell.Text;
            if (InputValue(value, context.Row, context.Col))
            {
                (_selectedCell.Children[1] as Label).Text = value;
                
                HighlightSameNumber(context);

                if(!string.IsNullOrEmpty(value))
                {
                    await CheckBoardState();
                    entryCell.Unfocus();
                }
            }
        }

        private void HighlightSameNumber(SudokuCellData context) {
            grid.Children.ForEach(c =>
            {
                var row = Grid.GetRow(c);
                var col = Grid.GetColumn(c);

                if (row != context.Row && context.Col != col)
                {
                    var cell = c as AbsoluteLayout;
                    var _ = cell.BindingContext as SudokuCellData;
                    
                    if (context.Value == null)
                    {
                        cell.BackgroundColor = GetBGAccent(row, col);
                    }
                    else if (_.Value == context.Value)
                    {
                        cell.BackgroundColor = Color.LightSalmon;
                    }
                }
            });
        }
    }
}