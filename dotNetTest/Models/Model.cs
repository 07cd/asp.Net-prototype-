using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Text;
using Google.Protobuf.Collections;
using Google.Cloud.Language.V1;
using System.Diagnostics;

//Query for resulting answer on question
//    SELECT n.word, v.word, question, answer FROM keysentence
//    JOIN question AS q ON q.id = question_id
//    JOIN answer AS a ON a.id = answer_id
//    JOIN noun_keysentence AS nk ON nk.id = keysentence.id
//    JOIN verb_keysentence AS vk ON vk.id = keysentence.id
//    JOIN noun AS n ON n.id = noun_id
//    JOIN verb AS v ON v.id = verb_id

namespace dotNetTest.Models
{
    public class Model
    {
        List<string> allNouns;
        List<string> allVerbs;
        List<string> allAnswers;
        List<string> allQuestions;

        List<string> lastVerbs = new List<string>();
        List<string> lastNouns = new List<string>();

        public string lastAnswer = "";
       
        string lastKeysentence = "";
        public string lastQuestion = "";

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

        /// <summary>
        /// Method inserts the answer into the database, if it exists get the id.
        /// LastAnswer is either set to the incoming variable or to the id,
        /// this will be checked as to insert it directly or search the id where the variable is.
        /// </summary>
       
        //Insert the answer, or if the answer already exists get id
        public void Insert(List<string> answer, string variable, string table, string column)
        {
            
            
            Connect();

              if (!answer.Contains(variable))
              {
                lastAnswer = variable;
                sql = "INSERT INTO " + table + "(" + column + ") VALUES('" + variable + "')";
                  Execute(sql);
              }
              else
              {
                  lastAnswer = Get("SELECT id FROM answer WHERE answer = '" + variable + "' ", 0).ToString();
              }

              Debug.WriteLine(lastAnswer);

            DisConnect();
        }




        /// <summary>
        /// Insert 
        /// </summary>
      
        public void InsertQuestion(string Answer, List<string> questions, List<string> nouns, List<string> verbs, string variable, string table, string column)
        {
            lastVerbs.Clear();
            lastNouns.Clear();

            Connect();
            
            allNouns = nouns;
            allVerbs = verbs;
            lastAnswer = Answer;

            string credential_path = @"D:\dotNetTest\dotNetTest\Adriaan-18cad82b0123.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
            if (!questions.Contains(variable))
            {
                lastQuestion = variable;
                string sql = "INSERT INTO " + table + "(" + column + ") VALUES('" + variable + "')";
                Execute(sql);
                AnalyzeIncomingData(variable);
               

            }
            else
            {
                // I'll have to see about setting a new keysentence and verb and noun key
                lastQuestion = Get("SELECT * FROM[" + table + "] WHERE " + column + " = '" + variable + "'", 0).ToString();
            }

            DisConnect();
        }


        public void Remove(string variable, string table, string column)
        {
            Connect();
            string sql = "";

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

                   
                        if (!allNouns.Contains(obj.text.content.ToString()) && obj.partOfSpeech.tag == "NOUN")
                        {
                        
                        sql = "INSERT INTO noun(word) VALUES('" + obj.text.content.ToString() + "')";
                            sentiments.Add(obj.text.content.ToString());
                            
                        
                            lastNouns.Add(obj.text.content.ToString());
                            Execute(sql);
                        }
                        else if (!allVerbs.Contains(obj.text.content.ToString()) && obj.partOfSpeech.tag == "VERB")
                        {
                        sql = "INSERT INTO verb(word) VALUES('" + obj.text.content.ToString() + "')";

                        sentiments.Add(obj.text.content.ToString());
                           
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
                                       lastVerbs.Add(dataReader.GetValue(0).ToString());
                                    }
                                    else if (obj.partOfSpeech.tag == "NOUN")
                                    {
                                        
                                        lastNouns.Add(dataReader.GetValue(0).ToString());
                                    }


                            }
                            dataReader.Close();
                        }
                    }
                }



            

            Debug.WriteLine($"Inserting {lastQuestion} {lastAnswer}");
            bool numericQuestion = int.TryParse(lastQuestion, out int lastQuestionId);
            bool numericAnswer = int.TryParse(lastAnswer, out int lastAnswerId);
            string answer = (numericAnswer) ? "'"+lastAnswer+"'" :"(SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "')";
            string question = (numericAnswer) ? "'"+lastQuestion+"'" : "(SELECT TOP 1 id FROM question WHERE question = '" + lastQuestion + "')";
            sql = $"INSERT INTO keysentence(question_id, answer_id) VALUES({question}, {answer})";
            Execute(sql);

    

           
          
           
            // Insert noun_keysentence and verb_keysentence
            foreach (var verb in lastVerbs)
            {
                // Check if they are id's or text, if they are id's, insert them directly, else search for the verb where the word = verb
                bool numericVerb = int.TryParse(verb, out int lastVerbId);
                sql = (numericVerb)? "INSERT INTO verb_keysentence(verb_id, keysentence_id) VALUES('" + verb + "', (SELECT id FROM keysentence WHERE answer_id = (SELECT id FROM answer WHERE answer = '" + lastAnswer + "')))" : "INSERT INTO verb_keysentence(verb_id, keysentence_id) VALUES( (SELECT TOP 1 id FROM verb WHERE word = '" + verb + "'), (SELECT id FROM keysentence WHERE answer_id = (SELECT id FROM answer WHERE answer = '" + lastAnswer + "')))";

                Execute(sql);
            }
            foreach (var noun in lastNouns)
            {
                // Same goes for here, except now for noun
                bool numericNoun = int.TryParse(noun, out int lastNounId);
                sql = (numericNoun) ? "INSERT INTO noun_keysentence(noun_id, keysentence_id) VALUES('" + noun + "', (SELECT id FROM keysentence WHERE answer_id = (SELECT id FROM answer WHERE answer = '" + lastAnswer + "')))" : "INSERT INTO noun_keysentence(noun_id, keysentence_id) VALUES( (SELECT TOP 1 id FROM noun WHERE word = '" + noun + "'), (SELECT id FROM keysentence WHERE answer_id = (SELECT id FROM answer WHERE answer = '" + lastAnswer + "')))";

                Execute(sql);
            }
           

            
        }

      












    }








}