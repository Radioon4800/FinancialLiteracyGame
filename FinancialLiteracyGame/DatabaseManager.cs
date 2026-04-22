using System;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Windows.Forms; // ДОБАВЬ ЭТУ СТРОКУ

public static class DatabaseManager
{
    // В классе DatabaseManager замените строку подключения:
    private static string connString = $"Data Source={AppDomain.CurrentDomain.BaseDirectory}game_data.db";

    public static void InitializeDatabase()
    {
        try
        {
            // 1. Гарантируем, что файл базы данных будет создан по правильному пути
            using (var conn = new SqliteConnection(connString))
            {
                conn.Open();

                // 2. Включаем поддержку внешних ключей (в SQLite она отключена по умолчанию)
                using (var pragmaCmd = new SqliteCommand("PRAGMA foreign_keys = ON;", conn))
                {
                    pragmaCmd.ExecuteNonQuery();
                }

                // 3. Создаем структуру таблиц одним блоком
                // Это гарантирует, что если база создалась, то в ней будут все нужные таблицы
                string sql = @"
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
                    steps_to_salary INTEGER DEFAULT 30,
                    cash_balance REAL,
                    passive_income REAL,
                    status TEXT DEFAULT 'Active',
                    last_saved TEXT,
                    FOREIGN KEY(player_id) REFERENCES Player(player_id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS Insurance (
                    insurance_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    session_id INTEGER,
                    type TEXT,
                    cost_per_month REAL,
                    is_active INTEGER,
                    FOREIGN KEY(session_id) REFERENCES GameSession(session_id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS RandomEvent (
                    event_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    session_id INTEGER,
                    month INTEGER,
                    type TEXT,
                    title TEXT,
                    effect_balance REAL,
                    effect_income REAL,
                    is_applied INTEGER,
                    FOREIGN KEY(session_id) REFERENCES GameSession(session_id) ON DELETE CASCADE
                );";

                using (var cmd = new SqliteCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 4. Финальная проверка: действительно ли таблица появилась в системном реестре SQLite
                using (var checkCmd = new SqliteCommand("SELECT name FROM sqlite_master WHERE type='table' AND name='GameSession';", conn))
                {
                    var result = checkCmd.ExecuteScalar();
                    if (result == null)
                    {
                        throw new Exception("База данных создана, но таблица 'GameSession' отсутствует. Проверьте права доступа к папке.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Выводим подробную ошибку, чтобы понять причину (права доступа, синтаксис и т.д.)
            MessageBox.Show("Критическая ошибка при инициализации базы данных:\n\n" + ex.Message,
                            "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    public static void SaveSession(int playerId, decimal cash, decimal passiveIncome, int month, int cell, int steps)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            string sql = @"
            UPDATE GameSession 
            SET cash_balance = @cash, 
                passive_income = @pi, 
                current_month = @month, 
                current_cell = @cell,
                steps_to_salary = @steps
            WHERE player_id = @pid";

            using (var cmd = new SqliteCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@cash", (double)cash);
                cmd.Parameters.AddWithValue("@pi", (double)passiveIncome);
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@cell", cell);
                cmd.Parameters.AddWithValue("@steps", steps);
                cmd.Parameters.AddWithValue("@pid", playerId);
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
                    // 1. СНАЧАЛА проверяем/создаем игрока (UPSERT)
                    // Если игрока с таким ID нет, INSERT его. Если есть — UPDATE.
                    string playerSql = @"
                    INSERT OR REPLACE INTO Player (player_id, name, profession, monthly_salary, monthly_expenses)
                    VALUES (@pid, @name, @prof, @sal, @exp);";

                    using (var pCmd = new SqliteCommand(playerSql, conn, transaction))
                    {
                        pCmd.Parameters.AddWithValue("@pid", playerId);
                        pCmd.Parameters.AddWithValue("@name", name);
                        pCmd.Parameters.AddWithValue("@prof", profession);
                        pCmd.Parameters.AddWithValue("@sal", (double)salary);
                        pCmd.Parameters.AddWithValue("@exp", (double)expenses);
                        pCmd.ExecuteNonQuery();
                    }

                    // 2. Теперь удаляем старую сессию (если была)
                    string deleteSession = "DELETE FROM GameSession WHERE player_id = @pid";
                    using (var dCmd = new SqliteCommand(deleteSession, conn, transaction))
                    {
                        dCmd.Parameters.AddWithValue("@pid", playerId);
                        dCmd.ExecuteNonQuery();
                    }

                    // 3. ТЕПЕРЬ создаем новую сессию. Ошибки FOREIGN KEY не будет, 
                    // так как Player с этим ID точно существует после шага 1.
                    string sql = @"INSERT INTO GameSession (player_id, cash_balance, passive_income, current_month, current_cell, last_saved, status) 
                               VALUES (@pid, @cash, 0, 1, 0, @date, 'Active')";

                    using (var cmd = new SqliteCommand(sql, conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("@pid", playerId);
                        cmd.Parameters.AddWithValue("@cash", (double)startCash);
                        cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Ошибка при создании сессии: " + ex.Message);
                }
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

    public static DataTable GetPlayer(int playerId)
    {
        using (var conn = new SqliteConnection(connString))
        {
            conn.Open();
            string sql = "SELECT * FROM Player WHERE player_id = @pid";
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
}