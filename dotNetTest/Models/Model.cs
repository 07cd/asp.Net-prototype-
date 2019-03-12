using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using Google.Protobuf.Collections;
using Google.Cloud.Language.V1;
using System.Diagnostics;

namespace dotNetTest.Models 
{
    public class Model
    {



        string lastNoun = "";
        string lastVerb = "";
        string lastKeysentence = "";

        private static List<string> sentiments = new List<string>();


        SqlConnection cnn;
        SqlDataAdapter adapter = new SqlDataAdapter();
    
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=prototype; Integrated Security=SSPI;";
        SqlCommand command;
        SqlDataReader dataReader;







        public void UserStats()
        {

        }
        public void SystemStats()
        {

        }







        // Connect to database
        public void Connect()
        {
   
            cnn = new SqlConnection(connectionString);
            cnn.Open();


        }
        // Disconnect to database 
        public void DisConnect()
        {
         
            command.Dispose();
            cnn.Close();
        }

        public void Execute(string sql)
        {

            command = new SqlCommand(sql, cnn);
            adapter.InsertCommand = command;
            adapter.InsertCommand.ExecuteNonQuery();
        }



        public List<string> Get(string query, int column)
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



        public void Insert(string variable, string table, string column)
        {
            Connect();

            string sql =
                "BEGIN IF NOT EXISTS(SELECT *  FROM ["+table+"] WHERE "+column+" = '"+variable+"') BEGIN INSERT INTO "+table+"("+column+") VALUES('" + variable+"') END END";



            Execute(sql);
            sql = "INSERT INTO keysentence_answer(sentence_id, answer_id) VALUES((SELECT TOP 1 id FROM keysentence ORDER BY id DESC), (SELECT TOP 1 id FROM answer ORDER BY id DESC))";
            Execute(sql);
            DisConnect();
        }





        public void InsertQuestion(string variable, string table, string column)
        {
            Connect();

            string credential_path = @"I:\C#\asp.net\dotNetTest\Adriaan-18cad82b0123.json";
            
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
            

            AnalyzeIncomingData(variable);

            string sql =
                "BEGIN IF NOT EXISTS(SELECT *  FROM [" + table + "] WHERE " + column + " = '" + variable + "') " +
                "BEGIN INSERT INTO " + table + "(" + column + ") VALUES('" + variable + "') END END";



            Execute(sql);
            DisConnect();
        }


        public void Remove(string variable, string table, string column)
        {
            Connect();
            string sql =
                "BEGIN IF NOT EXISTS(SELECT *  FROM [" + table + "] WHERE " + column + " = '" + variable + "') " +
                "BEGIN INSERT INTO " + table + "(" + column + ") VALUES('" + variable + "') END END";

            Execute(sql);
            DisConnect();
        }










        //Calls the Google NLP Api and throws the response into writeEntitySentiment function
        private void AnalyzeIncomingData(string text)
        {
            var client = LanguageServiceClient.Create();

            var response = client.AnalyzeSyntax(new Document()
            {
                Content = text,
                Type = Document.Types.Type.PlainText

            });

            WriteEntitySentiment(response.Tokens);
        }

        //Fills the Sentiments array with the entities values
        public void WriteEntitySentiment(RepeatedField<Token> tokens)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(tokens.ToString());
            string sql = "";
            foreach (var obj in jsonObj)
            {
                try
                {
                    
                    if(obj.partOfSpeech.tag == "VERB" || obj.partOfSpeech.tag == "NOUN"){
                        string table = obj.partOfSpeech.tag.ToString().ToLower();
                       
                      

                        sql =
                            "BEGIN IF NOT EXISTS(SELECT *  FROM [" + table + "] WHERE word = '" + obj.text.content.ToString() + "') BEGIN INSERT INTO " + table + "(word) VALUES('" + obj.text.content.ToString() + "') END END";
                        sentiments.Add(obj.text.content.ToString());



                        Execute(sql);
                        if (obj.partOfSpeech.tag == "VERB")
                        {
                            string lastVerb = obj.text.content.ToString();
                        }
                        else
                        {
                            string lastNoun = obj.text.content.ToString();
                        }





                    }






                }
                catch (Exception e)
                {
                    
                    Console.WriteLine(e);
                    throw;
                }
            }
            sql = "BEGIN IF NOT EXISTS (SELECT * FROM [keysentence] WHERE verb_id = (SELECT TOP 1 id FROM verb  WHERE word = '" + lastVerb + "' ORDER BY id DESC)" +
                " AND WHERE noun_id = (SELECT TOP 1 id FROM noun WHERE word = '" + lastNoun + "' ORDER BY id  DESC) " +
                "BEGIN INSERT INTO keysentence(verb_id, noun_id) VALUES((SELECT TOP 1 id FROM verb  WHERE word = '" + lastVerb + "' ORDER BY id DESC)," +
                " (SELECT TOP 1 id FROM noun WHERE word = '" + lastNoun + "' ORDER BY id  DESC)) END END";
            Execute(sql);
           
            
        }

        //An function to convert an array to string
        private static string ConvertArrayToString(string[] array)
        {
            var builder = new StringBuilder();
            foreach (var value in array)
            {
                builder.Append(value);
                builder.Append('.');
            }
            return builder.ToString();
        }












    }








}