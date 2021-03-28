using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Blog.Core.Common.DB
{
    public class BaseDBConfig
    {
        //private static string sqliteConnection = Appsettings.app(new string[] { "AppSettings", "Sqlite", "SqliteConnection" });
        //private static bool isSqliteEnabled = (Appsettings.app(new string[] { "AppSettings", "Sqlite", "Enabled" })).ObjToBool();

        //private static string sqlServerConnection = Appsettings.app(new string[] { "AppSettings", "SqlServer", "SqlServerConnection" });
        //private static bool isSqlServerEnabled = (Appsettings.app(new string[] { "AppSettings", "SqlServer", "Enabled" })).ObjToBool();

        //private static string mySqlConnection = Appsettings.app(new string[] { "AppSettings", "MySql", "MySqlConnection" });
        //private static bool isMySqlEnabled = (Appsettings.app(new string[] { "AppSettings", "MySql", "Enabled" })).ObjToBool();

        //private static string oracleConnection = Appsettings.app(new string[] { "AppSettings", "Oracle", "OracleConnection" });
        //private static bool IsOracleEnabled = (Appsettings.app(new string[] { "AppSettings", "Oracle", "Enabled" })).ObjToBool();

        public static (List<MutiDBOperate>, List<MutiDBOperate>) MutiConnectionString => MutiInitConn();
        //public static string ConnectionString => InitConn();
        public static DataBaseType DbType = DataBaseType.SqlServer;


        //private static string InitConn()
        //{
        //    if (isSqliteEnabled)
        //    {
        //        DbType = DataBaseType.Sqlite;
        //        return sqliteConnection;
        //    }
        //    else if (isSqlServerEnabled)
        //    {
        //        DbType = DataBaseType.SqlServer;
        //        return DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1.txt", @"c:\my-file\dbCountPsw1.txt", sqlServerConnection);
        //    }
        //    else if (isMySqlEnabled)
        //    {
        //        DbType = DataBaseType.MySql;
        //        return DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1_MySqlConn.txt", @"c:\my-file\dbCountPsw1_MySqlConn.txt", mySqlConnection);
        //    }
        //    else if (IsOracleEnabled)
        //    {
        //        DbType = DataBaseType.Oracle;
        //        return DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1_OracleConn.txt", @"c:\my-file\dbCountPsw1_OracleConn.txt", oracleConnection);
        //    }
        //    else
        //    {
        //        return "server=.;uid=sa;pwd=sa;database=WMBlogDB";
        //    }

        //}
        public static (List<MutiDBOperate>, List<MutiDBOperate>) MutiInitConn()
        {
            List<MutiDBOperate> listdatabase = Appsettings.app<MutiDBOperate>("DBS")
                .Where(i => i.Enabled).ToList();
            foreach (var i in listdatabase)
            {
                SpecialDbString(i);
            }
            List<MutiDBOperate> listdatabaseSimpleDB = new List<MutiDBOperate>();//单库
            List<MutiDBOperate> listdatabaseSlaveDB = new List<MutiDBOperate>();//从库

            // 单库，且不开启读写分离，只保留一个
            if (!Appsettings.app(new string[] { "CQRSEnabled" }).ObjToBool() && !Appsettings.app(new string[] { "MutiDBEnabled" }).ObjToBool())
            {
                if (listdatabase.Count == 1)
                {
                    return (listdatabase, listdatabaseSlaveDB);
                }
                else
                {
                    var dbFirst = listdatabase.FirstOrDefault(d => d.ConnId == Appsettings.app(new string[] { "MainDB" }).ObjToString());
                    if (dbFirst == null)
                    {
                        dbFirst = listdatabase.FirstOrDefault();
                    }
                    listdatabaseSimpleDB.Add(dbFirst);
                    return (listdatabaseSimpleDB, listdatabaseSlaveDB);
                }
            }


            // 读写分离，且必须是单库模式，获取从库
            if (Appsettings.app(new string[] { "CQRSEnabled" }).ObjToBool() && !Appsettings.app(new string[] { "MutiDBEnabled" }).ObjToBool())
            {
                if (listdatabase.Count > 1)
                {
                    listdatabaseSlaveDB = listdatabase.Where(d => d.ConnId != Appsettings.app(new string[] { "MainDB" }).ObjToString()).ToList();
                }
            }



            return (listdatabase, listdatabaseSlaveDB);
            //}
        }
        private static string DifDBConnOfSecurity(params string[] conn)
        {
            foreach (var item in conn)
            {
                try
                {
                    if (File.Exists(item))
                    {
                        return File.ReadAllText(item).Trim();
                    }
                }
                catch (System.Exception) { }
            }

            return conn[conn.Length - 1];
        }
        private static MutiDBOperate SpecialDbString(MutiDBOperate mutiDBOperate)
        {
            if (mutiDBOperate.DbType == DataBaseType.Sqlite)
            {
                mutiDBOperate.Connection = $"DataSource=" + Path.Combine(Environment.CurrentDirectory, mutiDBOperate.Connection);
            }
            //else if (mutiDBOperate.DbType == DataBaseType.SqlServer)
            //{
            //    mutiDBOperate.Conn = DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1.txt", @"c:\my-file\dbCountPsw1.txt", mutiDBOperate.Conn);
            //}
            else if (mutiDBOperate.DbType == DataBaseType.MySql)
            {
                mutiDBOperate.Connection = DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1_MySqlConn.txt", @"c:\my-file\dbCountPsw1_MySqlConn.txt", mutiDBOperate.Connection);
            }
            else if (mutiDBOperate.DbType == DataBaseType.Oracle)
            {
                mutiDBOperate.Connection = DifDBConnOfSecurity(@"D:\my-file\dbCountPsw1_OracleConn.txt", @"c:\my-file\dbCountPsw1_OracleConn.txt", mutiDBOperate.Connection);
            }

            return mutiDBOperate;
        }
    }

    public enum DataBaseType
    {
        MySql = 0,
        SqlServer = 1,
        Sqlite = 2,
        Oracle = 3,
        PostgreSQL = 4
    }
    public class MutiDBOperate
    {
        /// <summary>
        /// 连接启用开关
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// 连接ID
        /// </summary>
        public string ConnId { get; set; }
        /// <summary>
        /// 从库执行级别，越大越先执行
        /// </summary>
        public int HitRate { get; set; }
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string Connection { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DataBaseType DbType { get; set; }
    }
}
