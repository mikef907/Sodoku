using SQLite;
using Sudoku_UI.Models;
using SudokuUI.Persistence;
using System;
using System.Collections.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sudoku_UI.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GamesListPage : ContentPage
    {
        private SQLiteAsyncConnection db;
        private ObservableCollection<SudokuGame> _games;
        public GamesListPage()
        {
            InitializeComponent();
            db = DependencyService.Get<ISQLiteDb>().GetConnection();
        }

        protected async override void OnAppearing()
        {
            _games = new ObservableCollection<SudokuGame>();

            var games = await db.Table<SudokuGame>().ToListAsync();

            games.ForEach(game => _games.Add(game));

            gamesListView.ItemsSource = _games;

            base.OnAppearing();
        }

        private async void Copy_Seed_Clicked(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            var seed = menuItem.CommandParameter as int?;
            await Clipboard.SetTextAsync(seed.ToString());
            await DisplayAlert("Seed Copied", $"Seed {seed} copied to clipboard", "Gee, thanks");
        }

        private async void Delete_Clicked(object sender, EventArgs e)
        {
            var menuItem = sender as MenuItem;
            var game = menuItem.CommandParameter as SudokuGame;

            _games.Remove(game);
            await db.DeleteAsync(game);
        }
    }
}