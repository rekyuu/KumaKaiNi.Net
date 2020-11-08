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
