using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using DarkMultiPlayerServer;


namespace DMPPlayerLogging
{
    public class Main : DMPPlugin
    {
        //Settings file name
        protected const string SETTINGS_FILE = "PlayerLoggingSettings.txt";

        //Settings
        public static PlayerLoggingSettings settingsStore = new PlayerLoggingSettings();


        Dictionary<string, long> connectedPlayers = new Dictionary<string, long>();

        public Main()
        {
            LoadSettings();
            CommandHandler.RegisterCommand("reloadplayerlogging", ReloadSettings, "Reload the player logging settings");

            DBConnect connection = new DBConnect();

            string existsQuery = "show tables like @table;";
            string[,] existsParameters = { { "@table", settingsStore.tableName } };
            if (connection.Query(existsQuery, existsParameters).HasRows)
            {
                string creationSQL = "CREATE TABLE `ksp`.`" + settingsStore.tableName + "` ( `session_id` INT NOT NULL AUTO_INCREMENT , `session_player_name` VARCHAR(32) NOT NULL , `session_start_time` TIMESTAMP NOT NULL , `session_duration` INT NOT NULL , PRIMARY KEY (`session_id`)) ENGINE = InnoDB;";
                string[,] creationParameters = { { } };

                if (connection.NonQuery(creationSQL, creationParameters))
                {
                    DarkLog.Debug("DMPPlayerLogging: Created database table.");
                }
                else
                {
                    DarkLog.Error("DMPPlayerLogging: Logging table creation failed.");
                }
            }

            connection.CloseConnection();

        }

        private void ReloadSettings(string args)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            string settingsFileFullPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), SETTINGS_FILE);
            if (!File.Exists(settingsFileFullPath))
            {
                using (StreamWriter sw = new StreamWriter(settingsFileFullPath))
                {
                    sw.WriteLine("sqlServer = localhost:3306");
                    sw.WriteLine("dbName = ksp");
                    sw.WriteLine("dbUsername = kspgs");
                    sw.WriteLine("dbPassword = abc123");
                    sw.WriteLine("tableName = tbl-player-sessions");
                }
            }
            bool reloadSettings = false;
            using (StreamReader sr = new StreamReader(settingsFileFullPath))
            {
                string currentLine;

                while ((currentLine = sr.ReadLine()) != null)
                {
                    try
                    {
                        string key = currentLine.Substring(0, currentLine.IndexOf("=")).Trim();
                        string value = currentLine.Substring(currentLine.IndexOf("=") + 1).Trim();
                        switch (key)
                        {
                            case "sqlServer":
                                {
                                    string address = value.Substring(0, value.LastIndexOf(":"));
                                    string port = value.Substring(value.LastIndexOf(":") + 1);

                                    settingsStore.sqlIP = address;
                                    settingsStore.sqlPort = port;
                                }
                                break;
                            case "dbName":
                                settingsStore.dbName = value;
                                break;
                            case "dbUsername":
                                settingsStore.dbUsername = value;
                                break;
                            case "dbPassword":
                                settingsStore.dbPassword = value;
                                break;
                            case "tableName":
                                settingsStore.tableName = value;
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        DarkLog.Error("Error reading settings file, Exception " + e);
                    }
                }
            }
            if (reloadSettings)
            {
                //Load with the default settings if anything is incorrect.
                File.Delete(settingsFileFullPath);
                LoadSettings();
            }
        }

        public PlayerLoggingSettings getSettings()
        {
            return settingsStore;
        }
            
        public override void OnClientAuthenticated(ClientObject client)
        {
            long timeStamp = DarkMultiPlayerCommon.Common.GetCurrentUnixTime();

            connectedPlayers.Add(client.playerName, timeStamp);
            DarkLog.Debug("DMPPlayerLogging: Added " + client.playerName + " to the list of tracked players.");
        }

        public override void OnClientDisconnect(ClientObject client)
        {
            DBConnect connection = new DBConnect();

            long connectTime = connectedPlayers[client.playerName];
            long sessionDuration = DarkMultiPlayerCommon.Common.GetCurrentUnixTime() - connectTime;

            string sessionSendSQL = "INSERT INTO `"+ settingsStore.tableName + "` (`session_player_name`, `session_start_time`, `session_duration`) VALUES (@player_name, date_add('1970-01-01', INTERVAL @start SECOND), @duration)";
            string[,] parameters = { { "@player_name", client.playerName }, { "@start", connectTime.ToString() }, { "@duration", sessionDuration.ToString() } };

            if (connection.NonQuery(sessionSendSQL, parameters))
            {
                connectedPlayers.Remove(client.playerName);
                connection.CloseConnection();
                DarkLog.Debug("DMPPlayerLogging: Successfully sent " + client.playerName + "'s to the logging server.");
            }
            else
            {
                DarkLog.Error("DMPPlayerLogging: Couldn't send session data to logging database.");
            }
        }
    }
}

