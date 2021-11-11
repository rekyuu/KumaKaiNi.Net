using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KumaKaiNi.Core
{
    public abstract class DatabaseObject
    {
        public int Id = -1;
        public DateTime InsertedAt;
        public DateTime LastModified;

        public DatabaseObject() { }
        
        public DatabaseObject(Dictionary<string, object> row)
        {
            Dictionary<string, FieldInfo> fields = GetColumnToFieldMap();
            foreach (var (key, fieldInfo) in fields)
            {
                if (row.ContainsKey(key))
                {
                    object value = row[key];
                    if (fieldInfo.FieldType.IsEnum) value = Enum.Parse(fieldInfo.FieldType, value.ToString());
                    if (value is DBNull) value = null;

                    fieldInfo.SetValue(this, value);
                }
            }
        }

        public void Insert()
        {
            string tableName = GetTableName();

            if (Id != -1) throw new Exception("Object has likely already been inserted. Please use Update() instead.");
            if (!TableExists()) throw new Exception($"Database table '{tableName}' does not exist.");

            InsertedAt = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;

            Dictionary<string, FieldInfo> fields = GetColumnToFieldMap();
            fields.Remove("id");
            string[] values = new string[fields.Count];

            int i = 0;
            foreach (string column in fields.Keys)
            {
                values[i] = $"@{column}";
                i++;
            }

            using NpgsqlConnection connection = Database.DatabaseConnection();
            string sql = $"INSERT INTO {tableName} ({string.Join(", ", fields.Keys)}) VALUES ({string.Join(", ", values)}) RETURNING id";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            foreach (var (key, fieldInfo) in fields)
            {
                object value = fieldInfo.GetValue(this);
                if (fieldInfo.FieldType.IsEnum) value = value.ToString();

                command.Parameters.AddWithValue(key, value);
            }

            Id = (int)command.ExecuteScalar();
        }

        public void Update()
        {
            string tableName = GetTableName();

            if (Id == -1) throw new Exception("Object has likely not been inserted. Please use Insert() instead.");
            if (!TableExists()) throw new Exception($"Database table '{tableName}' does not exist.");

            LastModified = DateTime.UtcNow;

            Dictionary<string, FieldInfo> fields = GetColumnToFieldMap();
            fields.Remove("id");
            string[] values = new string[fields.Count];

            int i = 0;
            foreach (string column in fields.Keys)
            {
                values[i] = $"{column} = @{column}";
                i++;
            }

            using NpgsqlConnection connection = Database.DatabaseConnection();
            string sql = $"UPDATE {tableName} SET {string.Join(", ", values)} WHERE id = @id";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", Id);
            foreach (KeyValuePair<string, FieldInfo> field in fields)
            {
                object value = field.Value.GetValue(this);
                if (field.Value.FieldType.IsEnum) value = value.ToString();

                command.Parameters.AddWithValue(field.Key, value);
            }

            command.ExecuteNonQuery();
        }

        public void Delete()
        {
            string tableName = GetTableName();

            using NpgsqlConnection connection = Database.DatabaseConnection();
            string sql = $"DELETE FROM {tableName} WHERE id = @id";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", Id);

            command.ExecuteNonQuery();
        }

        public Dictionary<string, FieldInfo> GetColumnToFieldMap()
        {
            FieldInfo[] fields = GetType().GetFields();

            return fields.ToDictionary(field => Helpers.ToSnakeCase(field.Name));
        }

        public string GetTableName()
        {
            return Helpers.ToSnakeCase(GetType().Name + "s");
        }

        public bool TableExists()
        {
            string tableName = GetTableName();

            using NpgsqlConnection connection = Database.DatabaseConnection();

            using NpgsqlCommand command = new NpgsqlCommand("", connection);
            command.CommandText = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = @tableName)";
            command.Parameters.AddWithValue("tableName", tableName);

            return (bool)command.ExecuteScalar();
        }
    }
}
