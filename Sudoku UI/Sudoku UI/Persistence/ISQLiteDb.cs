using SQLite;

namespace SudokuUI.Persistence
{
    public interface ISQLiteDb
    {
        SQLiteAsyncConnection GetConnection();

    }
}
