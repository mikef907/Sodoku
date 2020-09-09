using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Sodoku_UI.Services;
using Sodoku_UI.Views;

namespace Sodoku_UI
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
