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
        Color gridColor = Color.FromHex("#894357");
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
            ShowWorkbench = false;

            gameStack.IsVisible = false;
            gameStack.HeightRequest = 0;
            startStack.IsVisible = false;
            startStack.HeightRequest = 0;

            ToolbarItems.Clear();

            _db = DependencyService.Get<ISQLiteDb>().GetConnection();

            seedEntry.Text = new Random().Next().ToString();

            StartOverCommand = new Command(async () =>
            {
                var seed = await DisplayPromptAsync("Start Over?", "Enter a seed value to generate a new puzzle.", initialValue: new Random().Next().ToString());

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

            Disappearing += SudokuPage_Disappearing;
            Appearing += SudokuPage_Appearing;

            Deserialize();
        }

        private async void Deserialize() {
            IsBusy = true;

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
            }
            else
            {
                startStack.IsVisible = true;
                startStack.HeightRequest = Device.GetNamedSize(NamedSize.Default, typeof(StackLayout));
            }

            IsBusy = false;
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

        private void SudokuPage_Appearing(object sender, EventArgs e)
        {
            InitGameTimer(reset: false);
        }

        private async void SudokuPage_Disappearing(object sender, EventArgs e)
        {
            _gameTimer?.StopTimer();
            _gameTimer = null;

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

        public async Task InitBoard(int? seed = null)
        {
            sudoku = new Sudoku();
            await sudoku.Init(seed);
        }

        public void InitGrid()
        {
            grid.Children.Clear();
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += TapGesture_Tapped;

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var stack = new AbsoluteLayout()
                    {
                        BackgroundColor = new Color(gridColor.R, gridColor.G, gridColor.B, GetAccent(i, j)),
                        BindingContext = sudoku.PuzzleBoard[i, j]
                    };

                    if (!sudoku.PuzzleBoard[i, j].Value.HasValue)
                    {
                        stack.GestureRecognizers.Add(tapGesture);
                    }

                    var guesses = new Label
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) / 1.4,
                        HeightRequest = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 1.8,
                        LineBreakMode = LineBreakMode.CharacterWrap,
                        Text = string.Join(" ", sudoku.PuzzleBoard[i, j].Data)
                    };

                    var entry = new Label()
                    {
                        Text = sudoku.PuzzleBoard[i, j].Value.ToString(),
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalOptions = LayoutOptions.End,
                        FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))
                    };


                    if (sudoku.PuzzleBoard[i, j].Value.HasValue)
                        entry.FontAttributes = FontAttributes.Bold;

                    stack.Children.Add(guesses, new Rectangle(0, 0, 1, 0.5), AbsoluteLayoutFlags.All);
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

            valueEntry.Text = sudoku.PuzzleBoard[context.Row, context.Col].Value?.ToString();
            ShowWorkbench = true;
        }

        private double GetAccent(int row, int col)
        {
            var evenRow = row / 3 == 0 || row / 3 == 2;
            var evenCol = col / 3 == 0 || col / 3 == 2;
            var center = row / 3 == 1 && col / 3 == 1;
            return (evenRow && evenCol) || center ? 0.1 : 0.2;
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
                sudoku.PuzzleBoard[row, col].Value = value;
                return true;
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

            var _ = values.Data.ToList();

            _.Sort();

            values.Data.Clear();

            foreach (int val in _)
            {
                values.Data.Add(val);
            }

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