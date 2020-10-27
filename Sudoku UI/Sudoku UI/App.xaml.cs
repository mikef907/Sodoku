using Xamarin.Forms;
using SudokuUI.Persistence;
using Sudoku_UI.Models;
using System;
using Sudoku_UI.Views;

namespace Sudoku_UI
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

        protected async override void OnStart()
        {
            var db = DependencyService.Get<ISQLiteDb>().GetConnection();
            await db.CreateTableAsync<SudokuGame>();
            await db.CreateTableAsync<CurrentGame>();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        protected async override void OnAppLinkRequestReceived(Uri uri)
        {
            if (uri.Host.EndsWith("playsudoku.app", StringComparison.OrdinalIgnoreCase))
            {

                if (uri.Segments != null && uri.Segments.Length == 3)
                {
                    var action = uri.Segments[1].Replace("/", "");
                    var val = uri.Segments[2];

                    switch (action)
                    {
                        case "seed":
                            int seed;

                            if (!string.IsNullOrEmpty(val) && int.TryParse(val, out seed))
                            {
                                await Shell.Current.GoToAsync($"//SudokuPage?seed={seed}");
                            }

                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}
