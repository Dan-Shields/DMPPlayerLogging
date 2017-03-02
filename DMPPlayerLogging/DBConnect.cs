﻿using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using DarkMultiPlayerServer;
using MySql.Data.MySqlClient;

namespace DMPPlayerLogging
{
    public class DBConnect
    {
        protected MySqlConnection conn;
        protected MySqlCommand cmd;
        PlayerLoggingSettings settingsStore = Main.settingsStore;

        public DBConnect()
        {
            string connectionString = "server=" + settingsStore.sqlIP + ";" +
                                    "uid=" + settingsStore.dbUsername + ";" +
                                    "pwd=" + settingsStore.dbPassword + ";" +
                                    "database=" + settingsStore.dbName + ";";

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = connectionString;
                conn.Open();
                DarkLog.Debug("Connection to logging database successful");
            }
            catch (MySqlException e)
            {
                DarkLog.Error("Error connecting to logging database " + e);
            }
        }

        public bool TableExists()
        {
            string existsQuery = "show tables like @table;";
            string[,] parameters = { {"@table", settingsStore.tableName} };
            if (Query(existsQuery, parameters).HasRows == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CreateTable()
        {
            string creationSQL = "CREATE TABLE `ksp`.`"+ settingsStore.tableName +"` ( `session_id` INT NOT NULL AUTO_INCREMENT , `session_player_name` VARCHAR(32) NOT NULL , `session_start_time` TIMESTAMP NOT NULL , `session_duration` INT NOT NULL , PRIMARY KEY (`session_id`)) ENGINE = InnoDB;";
            string[,] parameters = { { } };
            return NonQuery(creationSQL, parameters);

        }

        public QueryResult Query(string query, string[,] parameters)
        {
            try
            {
                cmd = new MySqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = query;

                for (int i = 0; i < parameters.Length /2; i++)
                {
                    cmd.Parameters.AddWithValue(parameters[i,0], parameters[i,1]);
                }

                cmd.Prepare();

                MySqlDataReader reader = null;

                reader = cmd.ExecuteReader();

                QueryResult result = new QueryResult();
                result.HasRows = reader.HasRows;

                if (reader.HasRows)
                {
                    DataTable data = new DataTable();
                    data.Load(reader);

                    result.Success = true;
                    result.Data = data;
                    return result;

                }
                else
                {
                    result.Success = true;
                    return result;
                }

            }
            catch (MySqlException e)
            {
                DarkLog.Error("Error querying the logging database " + e);
                QueryResult result = new QueryResult();
                result.Success = false;
                return result;
            }
        }

        public bool NonQuery(string query, string[,] parameters)
        {
            try
            {
                cmd = new MySqlCommand();
                cmd.Connection = conn;

                cmd.CommandText = query;

                for (int i = 0; i < parameters.Length/2; i++)
                {
                    cmd.Parameters.AddWithValue(parameters[i, 0], parameters[i, 1]);
                }

                cmd.Prepare();

                cmd.ExecuteNonQuery();

                return true;               

            }
            catch (MySqlException e)
            {
                DarkLog.Error("Error querying the logging database " + e);
                return false;
            }
        }

        public void CloseConnection()
        {
            conn.Close();
        }
        public void OpenConnection()
        {
            conn.Open();
        }
    }

    public class QueryResult
    {
        public bool Success { get; set; }
        public bool HasRows { get; set; }
        public DataTable Data { get; set; }
    }
}
