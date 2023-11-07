using System.Data.SQLite;

namespace WanseokBot;

public class Database
{
    private static SQLiteConnection Connection { get; set; } = null!;

    public static void Initialize()
    {
        CheckFileAndInitialize();

        Connection = new SQLiteConnection("Data Source=database.db;Version=3;");
        Connection.Open();

        var command = new SQLiteCommand("CREATE TABLE IF NOT EXISTS `meal_addresses` (`user_id` INTEGER NOT NULL, `address` TEXT NOT NULL, PRIMARY KEY(`user_id`))", Connection);
        command.ExecuteNonQuery();
    }

    private static void CheckFileAndInitialize()
    {
        if (File.Exists("database.db")) return;

        SQLiteConnection.CreateFile("database.db");
    }

    public static async Task AddMealAddress(ulong userId, string address)
    {
        var command = new SQLiteCommand("INSERT INTO `meal_addresses` (`user_id`, `address`) VALUES (@userId, @address)", Connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.Parameters.AddWithValue("@address", address);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task RemoveMealAddress(ulong userId)
    {
        var command = new SQLiteCommand("DELETE FROM `meal_addresses` WHERE `user_id` = @userId", Connection);
        command.Parameters.AddWithValue("@userId", userId);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<string?> GetMealAddress(ulong userId)
    {
        var command = new SQLiteCommand("SELECT `address` FROM `meal_addresses` WHERE `user_id` = @userId", Connection);
        command.Parameters.AddWithValue("@userId", userId);
        var reader = command.ExecuteReader();

        return await reader.ReadAsync() ? reader.GetString(0) : null;
    }
}