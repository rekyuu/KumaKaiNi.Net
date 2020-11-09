using Npgsql;
using Npgsql.Schema;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;
using System.Text;

namespace KumaKaiNi.Core
{
    public static class Database
    {
        public static void CreateTable<T>() where T : DatabaseObject
        {
            DatabaseObject obj = (DatabaseObject)Activator.CreateInstance(typeof(T));
            Dictionary<string, FieldInfo> fields = obj.GetColumnToFieldMap();
            fields.Remove("id");

            int i = 0;
            string tablename = obj.GetTableName();
            string[] columns = new string[fields.Count];
            foreach (KeyValuePair<string, FieldInfo> field in fields)
            {
                columns[i] = $"{field.Key} {GetPostgresType(field.Value.FieldType)}";
                i++;
            }

            using NpgsqlConnection connection = DatabaseConnection();

            string sql = $"CREATE TABLE IF NOT EXISTS {tablename}(id SERIAL PRIMARY KEY, {string.Join(", ", columns)})";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            command.ExecuteNonQuery();
        }

        public static void DropTable<T>() where T : DatabaseObject
        {
            DatabaseObject obj = (DatabaseObject)Activator.CreateInstance(typeof(T));
            string tablename = obj.GetTableName();

            using NpgsqlConnection connection = DatabaseConnection();

            string sql = $"DROP TABLE IF EXISTS {tablename}";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            command.ExecuteNonQuery();
        }

        public static List<T> GetMany<T>(string whereClause = "") where T : DatabaseObject
        {
            List<T> results = new List<T>();
            DatabaseObject obj = (DatabaseObject)Activator.CreateInstance(typeof(T));

            string tablename = obj.GetTableName();
            string sql = $"SELECT * FROM {tablename} {whereClause}";

            using NpgsqlConnection connection = DatabaseConnection();

            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);
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

        public static string GetPostgresType(Type type)
        {
            return (type.Name.ToLower()) switch
            {
                "bool" => "BOOLEAN",
                "datetime" => "TIMESTAMP",
                "decimal" => "NUMERIC",
                "double" => "DOUBLE PRECISION",
                "float" => "REAL",
                "guid" => "UUID",
                "int" => "INTEGER",
                "long" => "BIGINT",
                _ => "TEXT",
            };
        }
    }
}
