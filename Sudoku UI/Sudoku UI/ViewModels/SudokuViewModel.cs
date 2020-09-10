using Sudoku_Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Sudoku_UI.ViewModels
{
	public class SudokuViewModel : BaseViewModel
    {
        Sudoku sudoku = null;
        public bool showLabel { get => sudoku != null; }
        private string boardDisplay;

        public string BoardDisplay
        {
            get => boardDisplay;
            set => SetProperty(ref boardDisplay, value);
        }
        public SudokuViewModel()
        {
            StartCommand = new Command(async() =>
            {
                sudoku = new Sudoku();
                await ShowBoard();
            });
        }

        public Command StartCommand { get; }

        public async Task ShowBoard()
        {
            BoardDisplay = "Working...";

            await sudoku.Init();

            string _boardDisplay = "";

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var value = sudoku.PuzzleBoard[i, j];
                    _boardDisplay += value.HasValue ? $"| {value} " : "|    ";
                }
                _boardDisplay += $"|{Environment.NewLine}";
            }

            BoardDisplay = _boardDisplay;
        }
    }
}