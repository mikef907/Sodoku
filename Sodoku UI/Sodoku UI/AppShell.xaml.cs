using System;
using System.Collections.Generic;
using Sodoku_UI.ViewModels;
using Sodoku_UI.Views;
using Xamarin.Forms;

namespace Sodoku_UI
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

    }
}
