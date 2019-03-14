using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using Google.Protobuf.Collections;
using Google.Cloud.Language.V1;
using System.Diagnostics;


namespace dotNetTest.Models
{
    public class Model
    {
        List<string> allNouns;
        List<string> allVerbs;

        List<string> lastVerbs = new List<string>();
        List<string> lastNouns = new List<string>();


        string lastNoun = "";
        string lastVerb = "";
        string lastKeysentence = "";
        string lastAnswer = "";
        string lastQuestion = "";

        string sql = "";
        private static List<string> sentiments = new List<string>();


        SqlConnection cnn;
        SqlDataAdapter adapter = new SqlDataAdapter();
    
        string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=prototype; Integrated Security=SSPI;";
        SqlCommand command;
        SqlDataReader dataReader;







        public void UserStats(out List<string> questions, out List<string> count)
        {
            Connect();
            sql = "SELECT name, COUNT(question) FROM user_question AS uq JOIN [user] AS u ON (uq.user_id= u.id) JOIN [question] AS q ON (uq.question_id = q.id) GROUP BY name";
            questions = new List<string>(Get(sql, 0));
            count = new List<string>(Get(sql, 1));
           
            DisConnect();
        }
        public void SystemStats(out List<string> questions, out List<string> count)
        {
            Connect();
            sql = "SELECT question, COUNT(name) FROM user_question AS uq JOIN [user] AS u ON (uq.user_id= u.id) JOIN [question] AS q ON (uq.question_id = q.id) GROUP BY question";
            questions = new List<string>(Get(sql, 0));
            count = new List<string>(Get(sql, 1));

            DisConnect();
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



        public void Insert(List<string> answer, string variable, string table, string column)
        {
            
            Connect();
            if (!answer.Contains(variable))
            {
                sql = "INSERT INTO " + table + "(" + column + ") VALUES('" + variable + "')";
                Execute(sql);
            }
            else
            {
                sql = "SELECT id FROM " + table + " WHERE  " + column + " = '" + variable + "'";
                command = new SqlCommand(sql, cnn);

                dataReader = command.ExecuteReader();

                while (dataReader.Read())
                {

                    lastAnswer = dataReader.GetValue(0).ToString();

                }
                dataReader.Close();
            }

            DisConnect();
        }





        public void InsertQuestion(List<string> nouns, List<string> verbs, string variable, string table, string column)
        {
            Connect();

            allNouns = nouns;
            allVerbs = verbs;
            lastQuestion = variable;
            string credential_path = @"I:\C#\asp.net\dotNetTest\Adriaan-18cad82b0123.json";
            
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
            string sql =
              "BEGIN IF NOT EXISTS(SELECT *  FROM [" + table + "] WHERE " + column + " = '" + variable + "') " +
              "BEGIN INSERT INTO " + table + "(" + column + ") VALUES('" + variable + "') END END";

            Execute(sql);
            Debug.WriteLine(variable);
            AnalyzeIncomingData(variable);

          

            lastVerbs.Clear();
            lastNouns.Clear();

          
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
        
            foreach (var obj in jsonObj)
            {

         
                if (obj.partOfSpeech.tag == "VERB" || obj.partOfSpeech.tag == "NOUN"){
                        string table = obj.partOfSpeech.tag.ToString().ToLower();


                        if (!allNouns.Contains(obj.text.content.ToString()))
                        {
                        
                        sql = "INSERT INTO noun(word) VALUES('" + obj.text.content.ToString() + "')";
                            sentiments.Add(obj.text.content.ToString());
                            lastNoun = obj.text.content.ToString();
                        
                            lastNouns.Add(obj.text.content.ToString());
                            Execute(sql);
                        }
                        else if (!allVerbs.Contains(obj.text.content.ToString()))
                        {
                            sql =
                          "INSERT INTO verb(word) VALUES('" + obj.text.content.ToString() + "')";
                            sentiments.Add(obj.text.content.ToString());
                            lastVerb = obj.text.content.ToString();
                            lastVerbs.Add(obj.text.content.ToString());
                            Execute(sql);
                        }
                        else
                        {
                            sql = "SELECT id FROM " + table + " WHERE word = '" + obj.text.content.ToString() + "'";
                            command = new SqlCommand(sql, cnn);

                            dataReader = command.ExecuteReader();

                            while (dataReader.Read())
                            {
                                
                          
                                if (obj.partOfSpeech.tag == "VERB")
                                {
                                   lastVerb = dataReader.GetValue(0).ToString();
                                }
                                else
                                {
                                    lastNoun = dataReader.GetValue(0).ToString();
                                }


                            }
                            dataReader.Close();
                        }
                    }
                      
                    


                }
      
        
               


            // Checks if lastVerb and lastNoun are either id's, which would mean they are not new values, or if they are strings. 
            Debug.WriteLine(lastNoun + " " + lastVerb);
            bool numericNoun = int.TryParse(lastNoun, out int lastNounId);
            bool numericVerb = int.TryParse(lastVerb, out int lastVerbId);
            Debug.WriteLine(numericNoun + " " + numericVerb + " " + lastNounId + " " + lastVerbId);
            lastVerbs.ForEach(i => Debug.WriteLine("{0}\t", i));
            lastNouns.ForEach(i => Debug.WriteLine("{0}\t", i));
           
            // Insert noun_keysentence and verb_keysentence
            foreach (var verbs in lastVerbs)
            {
                sql = (numericVerb) ? "INSERT INTO verb_keysentence(verb_id) VALUES('" + lastVerbId + "')" : "INSERT INTO verb_keysentence(verb_id) VALUES((SELECT id FROM verb WHERE word = '"+lastVerb+"'))" ;

                Execute(sql);
            }
            foreach (var nouns in lastNouns)
            {
                sql = (numericNoun) ? "INSERT INTO noun_keysentence(noun_id) VALUES('" + lastNounId + "')" : "INSERT INTO noun_keysentence(noun_id) VALUES((SELECT id FROM noun WHERE word = '" + nouns + "'))";

                Execute(sql);
            }

            sql = "INSERT INTO keysentence(answer_id) VALUES((SELECT id FROM question WHERE question = '" + lastQuestion + "'))";
            Execute(sql);

            sql =  "UPDATE noun_keysentence SET keysentence_id = (SELECT id FROM keysentence WHERE answer_id = (SELECT id FROM question WHERE question = '" + lastQuestion + "')) WHERE keysentence_id = null;";

            Execute(sql);
            sql = "UPDATE verb_keysentence SET keysentence_id = (SELECT id FROM keysentence WHERE answer_id = (SELECT id FROM question WHERE question = 'How to delete a column')) WHERE keysentence_id = null;";
            Execute(sql);
        }

        //A function to convert an array to string
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