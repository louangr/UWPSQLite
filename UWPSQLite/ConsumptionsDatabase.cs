namespace UWPSQLite
{
    using Microsoft.Data.Sqlite;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Windows.Storage;

    public static class ConsumptionsDatabase
    {
        private const string databaseFilename = "consumptionLogs.db";
        private const string tableName = "ConsumptionLogs";

        public static async Task InitializeDatabase()
        {
            await ApplicationData.Current.LocalFolder.CreateFileAsync(databaseFilename, CreationCollisionOption.OpenIfExists);
            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseFilename);

            System.Diagnostics.Debug.WriteLine($"\n\n dbPath : {dbPath}\n\n");
            System.Diagnostics.Debug.WriteLine($"\n\n test : $Filename={dbPath}\n\n");

            using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
            {
                db.Open();

                string tableCommand = $"CREATE TABLE IF NOT EXISTS {tableName}" +
                    "(At TEXT NOT NULL PRIMARY KEY, " +
                    "mJOnBattery REAL NOT NULL, " +
                    "mJPluggedIn REAL NOT NULL)";

                SqliteCommand createTable = new SqliteCommand(tableCommand, db);

                createTable.ExecuteReader();
            }
        }

        public static async Task AddConsumptionLogs(List<Consumption> logsToAdd)
        {
            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseFilename);

            if (!File.Exists(dbPath))
            {
                await InitializeDatabase();
            }

            using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
            {
                db.Open();

                foreach (Consumption l in logsToAdd)
                {
                    try
                    {
                        SqliteCommand insertCommand = new SqliteCommand();
                        insertCommand.Connection = db;

                        insertCommand.CommandText = $"INSERT INTO {tableName} VALUES (@At, @mJOnBattery, @mJPluggedIn);";
                        insertCommand.Parameters.AddWithValue("@At", l.At.ToString("yyyy-MM-dd hh:mm:ss.fff"));
                        insertCommand.Parameters.AddWithValue("@mJOnBattery", l.mJOnBattery);
                        insertCommand.Parameters.AddWithValue("@mJPluggedIn", l.mJPluggedIn);

                        insertCommand.ExecuteReader();
                    }
                    catch (Exception ex)
                    {

                    }
                }

                db.Close();
            }
        }

        public static List<Consumption> SelectConsumptionLogs(DateTimeOffset olderCompleteLogFromPowerCfg)
        {
            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseFilename);
            var entries = new List<Consumption>();
            
            if (File.Exists(dbPath))
            {
                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();

                    try
                    {
                        SqliteCommand insertCommand = new SqliteCommand();
                        insertCommand.Connection = db;

                        insertCommand.CommandText = $"SELECT * FROM {tableName} WHERE At < '{olderCompleteLogFromPowerCfg.ToString("yyyy-MM-dd hh:mm:ss.fff")}';";

                        SqliteDataReader query = insertCommand.ExecuteReader();

                        while (query.Read())
                        {
                            Debug.WriteLine("\n\n" + query.GetString(0) + "");
                            Debug.WriteLine("" + query.GetString(1) + "");
                            Debug.WriteLine("" + query.GetString(2) + "\n\n");

                            entries.Add(new Consumption() { At = DateTimeOffset.Parse(query.GetString(0)), mJOnBattery = query.GetDouble(1), mJPluggedIn = query.GetDouble(2) });
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                    db.Close();
                }

            }
            
            return entries;
        }

        public static void DeleteConsumptionLogs(DateTimeOffset newerLogFromPowerCfg)
        {
            string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, databaseFilename);

            if (File.Exists(dbPath))
            {
                using (SqliteConnection db = new SqliteConnection($"Filename={dbPath}"))
                {
                    db.Open();

                    try
                    {
                        SqliteCommand insertCommand = new SqliteCommand();
                        insertCommand.Connection = db;
                        insertCommand.CommandText = $"DELETE FROM {tableName} WHERE At < '{newerLogFromPowerCfg.ToString("yyyy-MM-dd hh:mm:ss.fff")}';";

                        insertCommand.ExecuteReader();

                        //insertCommand.CommandText = $"SELECT * FROM {tableName} WHERE At < '{newerLogFromPowerCfg.ToString("yyyy-MM-dd hh:mm:ss.fff")}';";

                        //SqliteDataReader query = insertCommand.ExecuteReader();

                        //while (query.Read())
                        //{
                        //    Debug.WriteLine("\n\n" + query.GetString(0) + "\n\n");
                        //}

                    }
                    catch (Exception ex)
                    {

                    }


                    db.Close();
                }
            }
        }
    }
}
