using Xamarin.Forms;
using SudokuUI.Persistence;
using Sudoku_UI.Models;

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
    }
}
