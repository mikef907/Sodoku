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

        protected override void OnAppLinkRequestReceived(Uri uri)
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
                                MainPage.Navigation.PushAsync(new SudokuPage(seed));
                                //Device.BeginInvokeOnMainThread(async () =>
                                //{
                                //    await Current.MainPage.DisplayAlert("hello", val.Replace("&", " "), "ok");
                                //});
                            }

                            break;

                        default:
                            Xamarin.Forms.Device.OpenUri(uri);
                            break;
                    }
                }
            }
        }
    }
}
