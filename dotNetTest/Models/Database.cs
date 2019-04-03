using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using Google.Protobuf.Collections;
using Google.Cloud.Language.V1;
using System.Diagnostics;
using System.Linq;


namespace dotNetTest.Models
{
    public class Database
    {

        static string sql = "";

        static protected SqlConnection cnn;
        static protected SqlDataAdapter adapter = new SqlDataAdapter();

        static protected string connectionString = @"Data Source=DESKTOP-9T25K2E\SQLEXPRESS01;Initial Catalog=prototype; Integrated Security=SSPI;";
        static protected SqlCommand command;
        static protected SqlDataReader dataReader;




        /// <summary>
        /// Connect to the database with the connectionString
        /// </summary>
        public static void Connect()
        {

            
                cnn = new SqlConnection(connectionString);
                cnn.Open();
            
           
        }

        /// <summary>
        /// Execute the query from the string sql
        /// </summary>
        /// <param name="sql"></param>
        public static  void Execute(string sql)
        {
            command = new SqlCommand(sql, cnn);
            adapter.InsertCommand = command;
            adapter.InsertCommand.ExecuteNonQuery();
        }

        /// <summary>
        /// Get the output of a query in a List, execute the query from the string,
        /// the column gives which column it takes for the output (zero index), 0 is the first column and so on.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static List<string> Get(string query, int column)
        {
            Connect();
            List<string> Output = new List<string> { };

            command = new SqlCommand(query, cnn);

            dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {

                Output.Add(dataReader.GetValue(column).ToString());

            }

            dataReader.Close();
            DisConnect();

            return Output;
        }

        /// <summary>
        /// Disconnect to the database and dispose of the command
        /// </summary>
        public static void DisConnect()
        {
            command.Dispose();
            cnn.Close();
        }

    }
}