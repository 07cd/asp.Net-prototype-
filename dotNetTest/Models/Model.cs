using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace dotNetTest.Models
{
    public class Model
    {
        public List<string> Get(string query, int column)
        {
            string connetionString;
            SqlConnection cnn;
            connetionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=prototype; Integrated Security=SSPI;";
            cnn = new SqlConnection(connetionString);
            cnn.Open();
            List<string> Output = new List<string> { };

            SqlCommand command;
            SqlDataReader dataReader;


            string sql = query;

            command = new SqlCommand(sql, cnn);

            dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {

                Output.Add(dataReader.GetValue(column).ToString());

            }

            dataReader.Close();
            command.Dispose();
            cnn.Close();

            return Output;
        }
        public void Insert(string variable, string table, string column)
        {
            string connetionString;
            SqlConnection cnn;
            SqlDataAdapter adapter = new SqlDataAdapter();
            connetionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=prototype; Integrated Security=SSPI;";
            cnn = new SqlConnection(connetionString);
            cnn.Open();
         
            SqlCommand command;
            SqlDataReader dataReader;

     
            
            string sql =
                "BEGIN IF NOT EXISTS(SELECT *  FROM ["+table+"] WHERE "+column+" = '"+variable+"') BEGIN INSERT INTO "+table+"("+column+") VALUES('"+variable+"') END END";
      


            command = new SqlCommand(sql, cnn);
            adapter.InsertCommand = command;
            adapter.InsertCommand.ExecuteNonQuery();

            
            command.Dispose();
            cnn.Close();
        }
    }








}