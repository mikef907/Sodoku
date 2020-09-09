using System.ComponentModel;
using Xamarin.Forms;
using Sodoku_UI.ViewModels;

namespace Sodoku_UI.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}