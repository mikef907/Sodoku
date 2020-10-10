using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Sudoku_UI.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public ICommand TapCommand => new Command<string>((url) =>
        {
            Launcher.OpenAsync(new Uri(url));
        });
    }
}