using Npgsql;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace KumaKaiNi.Core
{
    public abstract class DatabaseObject
    {
        public int Id = -1;

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

                    field.Value.SetValue(this, value);
                }
            }
        }

        public void Insert()
        {
            if (Id != -1) throw new Exception("Object has likely already been inserted. Please use Update() instead.");

            string tablename = GetType().Name.ToLower() + "s";
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
            string sql = $"INSERT INTO {tablename} ({string.Join(", ", fields.Keys)}) VALUES ({string.Join(", ", values)})";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            foreach (KeyValuePair<string, FieldInfo> field in fields)
            {
                object value = field.Value.GetValue(this);
                if (field.Value.FieldType.IsEnum) value = value.ToString();

                command.Parameters.AddWithValue(field.Key, value);
            }

            command.ExecuteNonQuery();
        }

        public void Update()
        {
            if (Id == -1) throw new Exception("Object has likely not been inserted. Please use Insert() instead.");

            string tablename = GetType().Name.ToLower() + "s";
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
            string tablename = GetType().Name.ToLower() + "s";

            using NpgsqlConnection connection = Database.DatabaseConnection();
            string sql = $"DELETE FROM {tablename} WHERE id = @id";
            using NpgsqlCommand command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("id", Id);

            command.ExecuteNonQuery();
        }

        private Dictionary<string, FieldInfo> GetColumnToFieldMap()
        {
            Dictionary<string, FieldInfo> map = new Dictionary<string, FieldInfo>();
            FieldInfo[] fields = GetType().GetFields();

            for (int i = 0; i < fields.Length; i++)
            {
                map.Add(Helpers.ToSnakeCase(fields[i].Name), fields[i]);
            }

            return map;
        }
    }
}
