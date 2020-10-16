using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Sudoku_UI.Services;
using Sudoku_UI.Views;
using SudokuUI.Persistence;
using Sudoku_UI.Models;

namespace Sudoku_UI
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            var db = DependencyService.Get<ISQLiteDb>().GetConnection();
            db.CreateTableAsync<SudokuGame>();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
