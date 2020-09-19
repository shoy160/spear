using Spear.Core.Dependency;
using Spear.Core.Domain.Entities;
using Spear.Core.Extensions;
using Spear.Core.Reflection;
using Spear.Dapper.SQLite.Attributes;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Spear.Dapper.SQLite
{
    public static class SqliteExtensions
    {
        internal static string ConnectionString =
            "Data Source={0};Pooling=true;FailIfMissing=false;Version=3;UTF8Encoding=True;Journal Mode=Off;";
        public static object Default(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public static IDbConnection CreateConnection(this string path, string password = null)
        {
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = path,
                Version = 3,
                FailIfMissing = false,
                JournalMode = SQLiteJournalModeEnum.Off,
                Pooling = true,
                Password = password
            };
            if (!string.IsNullOrWhiteSpace(password))
                builder.Password = password;
            return new SQLiteConnection(builder.ConnectionString);
            //var connStr = string.Format(ConnectionString, path);
            //if (!string.IsNullOrWhiteSpace(password))
            //    connStr += $"Password={password};";
            //return connStr;
        }

        /// <summary> 自动创建数据库(自动备份) </summary>
        /// <param name="path">路径</param>
        /// <param name="password">密码(可选)</param>
        public static void AutoCreateDb(this string path, string password = null)
        {
            if (File.Exists(path))
            {
                //备份
                var dir = Path.GetDirectoryName(path);
                var name = Path.GetFileNameWithoutExtension(path);
                var ext = Path.GetExtension(path);
                var backupPath = $"{dir}/__old_{name}_{DateTime.Now:yyyyMMddHHmm}{ext}";
                var fi = new FileInfo(path);
                fi.MoveTo(backupPath);
            }

            var finder = CurrentIocManager.Resolve<ITypeFinder>();
            var tableTypes = finder.Find(t => t.BaseType == typeof(BaseEntity<string>)).Take(1);
            var sql = new StringBuilder();
            foreach (var tableType in tableTypes)
            {
                sql.AppendLine(tableType.CreateTableSql());
            }

            using (var conn = path.CreateConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        //设置密码
                        cmd.CommandText = $"PRAGMA key = {password};";
                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = sql.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary> 生成创建表sql语句 </summary>
        public static string CreateTableSql(this Type type)
        {
            var fields = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            var sql = new StringBuilder();
            sql.Append($"CREATE TABLE \"{type.PropName()}\" (");
            foreach (var field in fields)
            {
                var propType = field.PropertyType;
                if (propType.IsNullableType())
                    propType = propType.GetNonNummableType();
                sql.Append($"\"{field.PropName()}\" {propType.SqliteDbType()}");
                //不可为空
                var require = field.GetAttribute<RequiredAttribute>();
                if (require != null)
                {
                    sql.Append(" NOT NULL");
                }

                //主键
                var attr = field.GetAttribute<KeyAttribute>();
                if (attr != null)
                {
                    sql.Append(" PRIMARY KEY");
                }

                //默认值
                var def = field.GetAttribute<DefaultValueAttribute>();
                if (def != null)
                {
                    var value = def.Value ?? field.PropertyType.Default();
                    sql.Append($" DEFAULT {value}");
                }

                sql.Append(",");
            }

            return string.Concat(sql.ToString().TrimEnd(','), ");");
        }

        private static string SqliteDbType(this Type dbType)
        {
            if (dbType == typeof(int))
                return "INT";
            if (dbType == typeof(bool))
                return "BOOLEAN";
            if (dbType == typeof(DateTime))
                return "DATETIME";
            return "Text";
        }
    }
}
