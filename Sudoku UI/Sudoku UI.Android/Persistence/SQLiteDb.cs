using SQLite;
using SudokuUI.Persistence;
using System.IO;
using System;
using Xamarin.Forms;
using SudokuUI.Droid;

[assembly: Dependency(typeof(SQLiteDb))]
namespace SudokuUI.Droid
{
    public class SQLiteDb : ISQLiteDb
	{
		public SQLiteAsyncConnection GetConnection()
		{
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var path = Path.Combine(documentsPath, "MySQLite.db3");

			return new SQLiteAsyncConnection(path);
		}
	}
}