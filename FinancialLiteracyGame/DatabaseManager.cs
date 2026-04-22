using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Windows.Forms; // ДОБАВЬ ЭТУ СТРОКУ

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
                    current_cell INTEGER DEFAULT 0,
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
                // 2. ПРОВЕРЯЕМ И ДОБАВЛЯЕМ КОЛОНКУ (безопасный способ)
                bool hasCellColumn = false;
                using (var checkCmd = new SqliteCommand("PRAGMA table_info(GameSession);", conn))
                {
                    using (var reader = checkCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // reader["name"] — это имя колонки в таблице GameSession
                            if (reader["name"].ToString() == "current_cell")
                            {
                                hasCellColumn = true;
                                break;
                            }
                        }
                    }
                }

                if (!hasCellColumn)
                {
                    using (var alterCmd = new SqliteCommand("ALTER TABLE GameSession ADD COLUMN current_cell INTEGER DEFAULT 0;", conn))
                    {
                        alterCmd.ExecuteNonQuery();
                    }
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

    public static void SaveSession(int playerId, decimal cash, decimal passive, int month, int cell)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            // Используем INSERT, так как мы создаем записи истории или обновляем состояние
            string sql = @"INSERT INTO GameSession (player_id, cash_balance, passive_income, current_month, current_cell, last_saved) 
                       VALUES (@pid, @cash, @pass, @month, @cell, @date)";

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", playerId);
                cmd.Parameters.AddWithValue("@cash", (double)cash);
                cmd.Parameters.AddWithValue("@pass", (double)passive);
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@cell", cell);
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static void CreateNewSession(int playerId, string name, string profession, decimal salary, decimal expenses, decimal startCash)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // В будущем здесь будет логика выбора ID профиля, пока работаем с ID = 1

                    // 1. Очищаем старые события (опционально, если хотим полную очистку)
                    string clearEvents = "DELETE FROM RandomEvent WHERE session_id IN (SELECT session_id FROM GameSession WHERE player_id = @pid)";
                    using (var cmd = new SqliteCommand(clearEvents, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@pid", playerId);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Создаем новую стартовую запись сессии
                    string sql = @"INSERT INTO GameSession (player_id, cash_balance, passive_income, current_month, current_cell, last_saved) 
                               VALUES (@pid, @cash, 0, 1, 0, @date)";

                    using (var cmd = new SqliteCommand(sql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@pid", playerId);
                        cmd.Parameters.AddWithValue("@cash", (double)startCash);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch { transaction.Rollback(); throw; }
            }
        }
    }

    public static DataTable GetLastSession(int playerId)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            // Убедитесь, что имена колонок в SELECT совпадают с теми, что в CREATE TABLE
            string sql = "SELECT * FROM GameSession WHERE player_id = @pid ORDER BY session_id DESC LIMIT 1";
            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@pid", playerId);
                using (var reader = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(reader);
                    return dt;
                }
            }
        }
    }
    private static DataTable ExecuteQuery(string query, SqliteParameter[] parameters)
    {
        DataTable table = new DataTable();
        // Убедись, что переменная connectionString у тебя настроена на файл .db
        using (var conn = new SqliteConnection(connString))
        {
            try
            {
                conn.Open();
                using (var cmd = new SqliteCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (var reader = cmd.ExecuteReader())
                    {
                        table.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка БД: " + ex.Message);
                return null;
            }
        }
        return table;
    }
}