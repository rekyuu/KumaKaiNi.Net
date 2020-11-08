using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Database
    {
        public static void Init()
        {
            using NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString());
            connection.Open();

            using NpgsqlCommand command = new NpgsqlCommand();
            command.CommandText = @"CREATE TABLE quotes(id SERIAL PRIMARY KEY, text TEXT NOT NULL)";
            command.ExecuteNonQuery();

            command.CommandText = @"CREATE TABLE logs(id SERIAL PRIMARY KEY, timestamp TIMESTAMP, protocol INT, message TEXT, channel_is_private BOOL, channel_is_nsfw BOOL, user_is_admin BOOL)";
            command.ExecuteNonQuery();
        }

        public static List<T> GetResults<T>(string queryString)
        {
            List<T> results = new List<T>();

            using NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString());
            connection.Open();

            using NpgsqlCommand command = new NpgsqlCommand(queryString, connection);
            using NpgsqlDataReader reader = command.ExecuteReader();

            while(reader.Read())
            {
                T item = (T)Activator.CreateInstance(typeof(T), reader);
                results.Add(item);
            }

            return results;
        }

        public static string GetVersion()
        {
            using NpgsqlConnection connection = new NpgsqlConnection(GetConnectionString());
            connection.Open();

            string sql = "SELECT version()";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            string version = command.ExecuteScalar().ToString();

            return version;
        }

        public static string GetConnectionString()
        {
            return $"Host=localhost;Username=postgres;Password={ConfigurationManager.AppSettings.Get("PostgresSQLPassword")};Database=kumakaini";
        }
    }
}
