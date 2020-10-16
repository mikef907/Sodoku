using SQLite;
using Sudoku_UI.iOS;
using SudokuUI.Persistence;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteDb))]
namespace Sudoku_UI.iOS
{
    public class SQLiteDb : ISQLiteDb
	{
		public SQLiteAsyncConnection GetConnection()
		{
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var path = Path.Combine(documentsPath, "sudoku.db3");

			return new SQLiteAsyncConnection(path);
		}
	}
}