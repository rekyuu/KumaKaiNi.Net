using Npgsql;
using System;
using System.Collections.Generic;
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
            foreach (KeyValuePair<string, FieldInfo> field in fields)
            {
                if (row.ContainsKey(field.Key))
                {
                    object value = row[field.Key];
                    if (field.Value.FieldType.IsEnum) value = Enum.Parse(field.Value.FieldType, value.ToString());
                    if (value.GetType() == typeof(DBNull)) value = null;

                    field.Value.SetValue(this, value);
                }
            }
        }

        public void Insert()
        {
            string tablename = GetTableName();

            if (Id != -1) throw new Exception("Object has likely already been inserted. Please use Update() instead.");
            if (!TableExists()) throw new Exception($"Database table '{tablename}' does not exist.");

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
            string sql = $"INSERT INTO {tablename} ({string.Join(", ", fields.Keys)}) VALUES ({string.Join(", ", values)}) RETURNING id";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            foreach (KeyValuePair<string, FieldInfo> field in fields)
            {
                object value = field.Value.GetValue(this);
                if (field.Value.FieldType.IsEnum) value = value.ToString();

                command.Parameters.AddWithValue(field.Key, value);
            }

            Id = (int)command.ExecuteScalar();
        }

        public void Update()
        {
            string tablename = GetTableName();

            if (Id == -1) throw new Exception("Object has likely not been inserted. Please use Insert() instead.");
            if (!TableExists()) throw new Exception($"Database table '{tablename}' does not exist.");

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
            string sql = $"UPDATE {tablename} SET {string.Join(", ", values)} WHERE id = @id";
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
            string tablename = GetTableName();

            using NpgsqlConnection connection = Database.DatabaseConnection();
            string sql = $"DELETE FROM {tablename} WHERE id = @id";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", Id);

            command.ExecuteNonQuery();
        }

        public Dictionary<string, FieldInfo> GetColumnToFieldMap()
        {
            Dictionary<string, FieldInfo> map = new Dictionary<string, FieldInfo>();
            FieldInfo[] fields = GetType().GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                map.Add(Helpers.ToSnakeCase(fields[i].Name), fields[i]);
            }

            return map;
        }

        public string GetTableName()
        {
            return Helpers.ToSnakeCase(GetType().Name + "s");
        }

        public bool TableExists()
        {
            string tablename = GetTableName();

            using NpgsqlConnection connection = Database.DatabaseConnection();

            using NpgsqlCommand command = new NpgsqlCommand("", connection);
            command.CommandText = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = @tablename)";
            command.Parameters.AddWithValue("tablename", tablename);

            return (bool)command.ExecuteScalar();
        }
    }
}
