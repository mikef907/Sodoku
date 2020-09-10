using System.ComponentModel;
using Xamarin.Forms;
using Sudoku_UI.ViewModels;

namespace Sudoku_UI.Views
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