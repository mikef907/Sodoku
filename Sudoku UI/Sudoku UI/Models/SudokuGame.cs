using SQLite;
using Sudoku_Lib;
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

    public class CurrentGame
    { 
        [PrimaryKey]
        public int? Id { get; set; }
        public int Seed { get; set; }
        public string State { get; set; }
        public TimeSpan Timer { get; set; }
    }
}
