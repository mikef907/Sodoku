using System;
using System.Collections.Generic;
using Sudoku_UI.ViewModels;
using Sudoku_UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Sudoku_UI
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            VersionTracking.Track();
        }
    }
}
