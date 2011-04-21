using System;
using System.Configuration;
using System.Windows.Media.Media3D;
using Common.Logging;
using Strive.Common;
using Strive.Server.DB;


namespace Strive.Server.Logic
{
    /// <summary>
    /// Summary description for Global.
    /// </summary>
    public class Global
    {
        public static Random Rand = new Random();
        public static DateTime Now = DateTime.Now;
        public static Vector3D Up = new Vector3D(0, 1, 0);
        public static Schema Schema;

        public static int WorldId;
        public static int Port;
        public static string LogFilename;
        public static string WorldFilename;
        public static string ConnectionString;

        static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public static void ReadConfiguration()
        {
            // mandatory fields
            string s = ConfigurationManager.AppSettings["WorldID"];
            if (s == null)
            {
                Log.Error("world_id missing in configuration");
                WorldId = 1;
            }
            else
            {
                WorldId = int.Parse(s);
            }

            s = ConfigurationManager.AppSettings["Port"];
            if (s == null)
            {
                Log.Error("Port missing in configuration");
                Port = Constants.DefaultPort;
            }
            else
            {
                Port = int.Parse(s);
            }

            // optional fields
            LogFilename = ConfigurationManager.AppSettings["LogFileName"];

            // one and one only of these two should be specified.
            WorldFilename = ConfigurationManager.AppSettings["WorldFileName"];
            ConnectionString = ConfigurationManager.AppSettings["ConnectionString"];
        }
    }
}

