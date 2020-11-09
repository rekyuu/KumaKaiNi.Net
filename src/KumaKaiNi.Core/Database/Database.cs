using Npgsql;
using Npgsql.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Database
    {
        public static void Init()
        {
            using NpgsqlConnection connection = DatabaseConnection();

            using NpgsqlCommand command = new NpgsqlCommand("", connection);
            command.CommandText = @"DROP TABLE IF EXISTS logs";
            command.ExecuteNonQuery();

            command.CommandText = @"DROP TABLE IF EXISTS quotes";
            command.ExecuteNonQuery();

            command.CommandText = @"DROP TABLE IF EXISTS custom_commands";
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE IF NOT EXISTS logs(id SERIAL PRIMARY KEY, timestamp TIMESTAMP, protocol VARCHAR(255), message TEXT, message_id VARCHAR(255), user_id VARCHAR(255), channel_id VARCHAR(255))";
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE IF NOT EXISTS quotes(id SERIAL PRIMARY KEY, text TEXT NOT NULL)";
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE IF NOT EXISTS custom_commands(id SERIAL PRIMARY KEY, command VARCHAR(255), response TEXT)";
            command.ExecuteNonQuery();
        }

        public static List<T> GetResults<T>(string queryString = "") where T : DatabaseObject
        {
            List<T> results = new List<T>();

            string tablename = typeof(T).Name.ToLower() + "s";
            if (queryString == "") queryString = $"SELECT * FROM {tablename}";

            using NpgsqlConnection connection = DatabaseConnection();

            using NpgsqlCommand command = new NpgsqlCommand(queryString, connection);
            using NpgsqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                ReadOnlyCollection<NpgsqlDbColumn> columns = reader.GetColumnSchema();

                while (reader.Read())
                {
                    Dictionary<string, object> row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(columns[i].ColumnName, reader[i]);
                    }

                    T item = (T)Activator.CreateInstance(typeof(T), new object[] { row });
                    results.Add(item);
                }
            }

            return results;
        }

        public static string GetVersion()
        {
            using NpgsqlConnection connection = DatabaseConnection();

            string sql = "SELECT version()";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            string version = command.ExecuteScalar().ToString();

            return version;
        }

        public static NpgsqlConnection DatabaseConnection()
        {
            NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString());
            connection.Open();

            return connection;
        }

        public static string GetConnectionString()
        {
            return $"Host=localhost;Username=postgres;Password={ConfigurationManager.AppSettings.Get("PostgresSQLPassword")};Database=kumakaini";
        }
    }
}
