using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Sudoku_UI.Services;
using Sudoku_UI.Views;

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
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
