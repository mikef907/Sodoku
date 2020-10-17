using Xamarin.Essentials;

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
