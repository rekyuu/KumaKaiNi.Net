using Npgsql;
using Npgsql.Schema;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            string tableName = obj.GetTableName();
            string[] columns = new string[fields.Count];
            foreach (var (key, value) in fields)
            {
                columns[i] = $"{key} {GetPostgresType(value.FieldType)}";
                i++;
            }

            using NpgsqlConnection connection = DatabaseConnection();

            string sql = $"CREATE TABLE IF NOT EXISTS {tableName}(id SERIAL PRIMARY KEY, {string.Join(", ", columns)})";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            command.ExecuteNonQuery();
        }

        public static void DropTable<T>() where T : DatabaseObject
        {
            DatabaseObject obj = (DatabaseObject)Activator.CreateInstance(typeof(T));
            string tableName = obj.GetTableName();

            using NpgsqlConnection connection = DatabaseConnection();

            string sql = $"DROP TABLE IF EXISTS {tableName}";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            command.ExecuteNonQuery();
        }

        public static List<T> GetMany<T>(WherePredicate[] wherePredicates = null, int limit = 0, bool randomOrder = false) where T : DatabaseObject
        {
            List<T> results = new List<T>();
            DatabaseObject obj = (DatabaseObject)Activator.CreateInstance(typeof(T));

            string tableName = obj.GetTableName();
            string sql = $"SELECT * FROM {tableName}";

            StringBuilder sb = new StringBuilder();
            if (wherePredicates != null)
            {
                sb.Append(" WHERE ");

                int i = 0;
                foreach (WherePredicate wherePredicate in wherePredicates)
                {
                    sb.Append(wherePredicate.Target.GetType().IsArray
                        ? $"{wherePredicate.Source} {wherePredicate.Comparitor} (@{wherePredicate.Source}{i}) AND "
                        : $"{wherePredicate.Source} {wherePredicate.Comparitor} @{wherePredicate.Source}{i} AND ");

                    i++;
                }

                string whereClause = sb.ToString();
                sql += whereClause.Remove(whereClause.Length - 5, 5);
            }

            if (randomOrder) sql += " ORDER BY RANDOM()";
            else sql += " ORDER BY last_modified DESC";

            if (limit > 0) sql += " LIMIT @limit";

            using NpgsqlConnection connection = DatabaseConnection();
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            if (wherePredicates != null)
            {
                int i = 0;
                foreach (WherePredicate wherePredicate in wherePredicates)
                {
                    object target = wherePredicate.Target;
                    if (wherePredicate.Target.GetType().IsEnum) target = target.ToString();

                    command.Parameters.AddWithValue($"{wherePredicate.Source}{i}", target);
                    i++;
                }
            }

            if (limit > 0) command.Parameters.AddWithValue("limit", limit);

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

        private static string GetConnectionString()
        {
            string host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
            string username = Environment.GetEnvironmentVariable("POSTGRES_USERNAME");
            string password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
            string database = Environment.GetEnvironmentVariable("POSTGRES_DATABASE");
            
            return $"Host={host};Username={username};Password={password};Database={database}";
        }

        private static string GetPostgresType(Type type)
        {
            return type.Name.ToLower() switch
            {
                "boolean" => "BOOLEAN",
                "datetime" => "TIMESTAMP",
                "decimal" => "NUMERIC",
                "double" => "DOUBLE PRECISION",
                "single" => "REAL",
                "guid" => "UUID",
                "int32" => "INTEGER",
                "int64" => "BIGINT",
                _ => "TEXT",
            };
        }
    }
}
