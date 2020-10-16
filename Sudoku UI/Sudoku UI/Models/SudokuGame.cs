using SQLite;
using System;

namespace Sudoku_UI.Models
{
    public class SudokuGame
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public TimeSpan Time { get; set; }
        [Indexed]
        public int Seed { get; set; }
        public int Attempt { get; set; }
        public bool Solved { get; set; }
    }
}
