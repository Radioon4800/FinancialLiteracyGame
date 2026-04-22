using System;
using Microsoft.Data.Sqlite;

public static class DatabaseManager
{
    private static string connString = "Data Source=game_data.db";

    public static void InitializeDatabase()
    {
        try
        {
            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();

                // 1. Сначала создаем все таблицы в их базовом виде
                string createTablesSql = @"
                CREATE TABLE IF NOT EXISTS Player (
                    player_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    profession TEXT,
                    monthly_salary REAL,
                    monthly_expenses REAL
                );

                CREATE TABLE IF NOT EXISTS GameSession (
                    session_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    player_id INTEGER,
                    current_month INTEGER DEFAULT 1,
                    cash_balance REAL,
                    passive_income REAL,
                    status TEXT,
                    last_saved TEXT,
                    FOREIGN KEY(player_id) REFERENCES Player(player_id)
                );

                CREATE TABLE IF NOT EXISTS Insurance (
                    insurance_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    session_id INTEGER,
                    type TEXT,
                    cost_per_month REAL,
                    is_active INTEGER,
                    FOREIGN KEY(session_id) REFERENCES GameSession(session_id)
                );

                CREATE TABLE IF NOT EXISTS RandomEvent (
                    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    session_id INTEGER,
                    month INTEGER,
                    title TEXT,
                    effect_balance REAL,
                    effect_income REAL,
                    is_applied INTEGER,
                    FOREIGN KEY(session_id) REFERENCES GameSession(session_id)
                );";

                using (var cmd = new SqliteCommand(createTablesSql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 2. ТЕПЕРЬ проверяем и добавляем колонку type, если её нет
                try
                {
                    using (var checkCmd = new SqliteCommand("SELECT type FROM RandomEvent LIMIT 1;", conn))
                    {
                        checkCmd.ExecuteReader().Close();
                    }
                }
                catch
                {
                    using (var alterCmd = new SqliteCommand("ALTER TABLE RandomEvent ADD COLUMN type TEXT;", conn))
                    {
                        alterCmd.ExecuteNonQuery();
                    }
                }

                // 3. Включаем ключи и создаем игрока
                using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn))
                {
                    pragmaCmd.ExecuteNonQuery();
                }

                string checkPlayer = "INSERT OR IGNORE INTO Player (player_id, name, profession, monthly_salary, monthly_expenses) VALUES (1, 'Алексей', 'Программист', 50000, 30000);";
                using (var playerCmd = new SqliteCommand(checkPlayer, conn))
                {
                    playerCmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.Forms.MessageBox.Show("Ошибка инициализации БД: " + ex.Message);
        }
    }

    // ИСПРАВЛЕНИЕ CS0117: GetCurrentSessionId
    public static int GetCurrentSessionId(int playerId)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            string sql = "SELECT session_id FROM GameSession WHERE player_id = @pid AND status = 'Active' LIMIT 1";
            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", playerId);
                var result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }

    // ИСПРАВЛЕНИЕ CS0117: HasActiveInsurance
    public static bool HasActiveInsurance(int sessionId, string type)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            string sql = "SELECT COUNT(*) FROM Insurance WHERE session_id = @sid AND type = @type AND is_active = 1";
            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@sid", sessionId);
                cmd.Parameters.AddWithValue("@type", type);
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }

    public static void LogRandomEvent(int sessionId, int month, string type, string title, decimal balanceEffect, decimal incomeEffect)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            // Добавляем поле type и effect_income, чтобы видеть влияние на пассивный доход
            string sql = @"INSERT INTO RandomEvent (session_id, month, type, title, effect_balance, effect_income, is_applied) 
                       VALUES (@sid, @month, @type, @title, @effB, @effI, 1)";

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@sid", sessionId);
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@type", type); // 'Risk', 'Market', etc.
                cmd.Parameters.AddWithValue("@title", title);
                cmd.Parameters.AddWithValue("@effB", (double)balanceEffect);
                cmd.Parameters.AddWithValue("@effI", (double)incomeEffect);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static void SaveSession(int playerId, decimal cash, decimal passiveIncome, int month)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn)) { pragmaCmd.ExecuteNonQuery(); }

            string sql = @"UPDATE GameSession 
                           SET cash_balance = @cash, 
                               passive_income = @passive, 
                               current_month = @month, 
                               last_saved = @time 
                           WHERE player_id = @pid AND status = 'Active'";

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@cash", (double)cash);
                cmd.Parameters.AddWithValue("@passive", (double)passiveIncome);
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                cmd.Parameters.AddWithValue("@pid", playerId);

                if (cmd.ExecuteNonQuery() == 0)
                {
                    CreateNewSession(playerId, cash, passiveIncome, conn);
                }
            }
        }
    }

    private static void CreateNewSession(int playerId, decimal cash, decimal passive, SqliteConnection conn)
    {
        string sql = @"INSERT INTO GameSession (player_id, cash_balance, passive_income, status, current_month) 
                       VALUES (@pid, @cash, @passive, 'Active', 1)";
        using (var cmd = new SqliteCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@pid", playerId);
            cmd.Parameters.AddWithValue("@cash", (double)cash);
            cmd.Parameters.AddWithValue("@passive", (double)passive);
            cmd.ExecuteNonQuery();
        }
    }
}