using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using DarkMultiPlayerServer;
using 

namespace DMPPlayerLogging
{
    public class Main : DMPPlugin
    {
        //Settings file name

        private const string SETTINGS_FILE = "PlayerLoggingSettings.txt";
        //Uses explicit threads - The async methods whack out for some people in some rare cases it seems.
        //Plus, we can explicitly terminate the thread to kill the connection upon shutdown.
        private Thread loadThread;
        //Settings
        PlayerLoggingSettings settingsStore = new PlayerLoggingSettings();

        public Main()
        {
            loadThread = new Thread(new ThreadStart(LoadSettings));
            loadThread.Start();
            CommandHandler.RegisterCommand("reloadplayerlogging", ReloadSettings, "Reload the player logging settings");
        }

        private void ReloadSettings(string args)
        {
            loadedSettings = false;
            loadThread = new Thread(new ThreadStart(LoadSettings));
            loadThread.Start();
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
                    sw.WriteLine("tableName = tbl-player-logging");
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

        public override void OnClientAuthenticated(ClientObject client)
        {
            //connectedPlayers.Add(client.playerName);
        }

        public override void OnClientDisconnect(ClientObject client)
        {
            //connectedPlayers.Remove(client.playerName);
        }

        public override void OnServerStop()
        {
            
        }

        private void DBConnect()
        {
            SqlConnection dbh = new SqlConnection("server=" + settingsStore.sqlIP + ";" +
                                    "uid=" + settingsStore.dbUsername + ";" +
                                    "pwd=" + settingsStore.dbPassword + ";" +
                                    "database=" + settingsStore.dbName + ";");

            try
            {
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = myConnectionString;
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

