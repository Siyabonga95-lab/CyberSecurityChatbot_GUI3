using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace CyberSecurityChatbot_GUI3
{
    // ============================================================
    // DATABASE MANAGER — Handles all SQLite operations
    //
    // WHY I Used SQLITE?
    //   No server needed — the database is a single .db file stored
    //   in the application folder. Perfect for a student project.
    //
    // CRUD OPERATIONS COVERED (for full marks):
    //   CREATE  — AddTask()
    //   READ    — GetAllTasks()
    //   UPDATE  — MarkTaskComplete(), UpdateReminder()
    //   DELETE  — DeleteTask()
    // ============================================================
    public class DatabaseManager
    {
        // Path to the SQLite database file — stored next to the .exe
        private readonly string _dbPath;
        private readonly string _connectionString;

        public DatabaseManager()
        {
            // Store the .db file in the same folder as the running executable
            string exeFolder = AppDomain.CurrentDomain.BaseDirectory;
            _dbPath = Path.Combine(exeFolder, "cybertasks.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";

            InitialiseDatabase();
        }

        // ── INITIALISE ──────────────────────────────────────────
        // Creates the Tasks table if it doesn't already exist.
        // Called once on startup.
        private void InitialiseDatabase()
        {
            // SQLiteConnection.CreateFile(_dbPath) would overwrite — check first
            if (!File.Exists(_dbPath))
                SQLiteConnection.CreateFile(_dbPath);

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string createTable = @"
                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title       TEXT    NOT NULL,
                        Description TEXT    NOT NULL,
                        Reminder    TEXT    DEFAULT '',
                        IsCompleted INTEGER DEFAULT 0,
                        CreatedAt   TEXT    NOT NULL
                    );";
                using (var cmd = new SQLiteCommand(createTable, conn))
                    cmd.ExecuteNonQuery();
            }
        }

        // ── CREATE — ADD TASK ────────────────────────────────────
        // Inserts a new task and returns the auto-generated Id.
        public int AddTask(TaskItem task)
        {
            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    INSERT INTO Tasks (Title, Description, Reminder, IsCompleted, CreatedAt)
                    VALUES (@title, @desc, @reminder, 0, @created);
                    SELECT last_insert_rowid();";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@title", task.Title);
                    cmd.Parameters.AddWithValue("@desc", task.Description);
                    cmd.Parameters.AddWithValue("@reminder", task.Reminder ?? "");
                    cmd.Parameters.AddWithValue("@created", task.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

                    object result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
        }

        // ── READ — GET ALL TASKS ─────────────────────────────────
        // Returns every task in the database (completed or not).
        public List<TaskItem> GetAllTasks()
        {
            var tasks = new List<TaskItem>();

            using (var conn = new SQLiteConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT Id, Title, Description, Reminder, IsCompleted, CreatedAt FROM Tasks ORDER BY Id DESC;";

                using (var cmd = new SQLiteCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new TaskItem
                        {
                            Id = reader.GetInt32(0),
                            Title = reader.GetString(1),
                            Description = reader.GetString(2),
                            Reminder = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            IsCompleted = reader.GetInt32(4) == 1,
                            CreatedAt = DateTime.Parse(reader.GetString(5))
                        });
                    }
                }
            }
            return tasks;
        }

        // ── UPDATE — MARK AS COMPLETE ────────────────────────────
        public bool MarkTaskComplete(int id)
        {
            return ExecuteNonQuery(
                "UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id;",
                ("@id", id));
        }

        // ── UPDATE — SET / CHANGE REMINDER ──────────────────────
        public bool UpdateReminder(int id, string reminder)
        {
            return ExecuteNonQuery(
                "UPDATE Tasks SET Reminder = @reminder WHERE Id = @id;",
                ("@reminder", reminder), ("@id", id));
        }

        // ── DELETE — REMOVE TASK ─────────────────────────────────
        public bool DeleteTask(int id)
        {
            return ExecuteNonQuery(
                "DELETE FROM Tasks WHERE Id = @id;",
                ("@id", id));
        }

        // ── HELPER — EXECUTE NON-QUERY ───────────────────────────
        // Runs INSERT / UPDATE / DELETE and returns true on success.
        private bool ExecuteNonQuery(string sql, params (string key, object val)[] parameters)
        {
            try
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        foreach (var (key, val) in parameters)
                            cmd.Parameters.AddWithValue(key, val);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Error: {ex.Message}");
                return false;
            }
        }
    }
}