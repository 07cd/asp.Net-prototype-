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
    public class Model
    {
        List<string> allNouns;
        List<string> allVerbs;
        List<string> allAnswers;
        List<string> allQuestions;

        List<string> lastVerbs = new List<string>();
        List<string> lastNouns = new List<string>();

        public string lastAnswer = "";
       
       
        public string lastQuestion = "";

        string sql = "";
        private static List<string> sentiments = new List<string>();

        SqlConnection cnn;
        SqlDataAdapter adapter = new SqlDataAdapter();
    
        string connectionString = @"Data Source=DESKTOP-9T25K2E\SQLEXPRESS01;Initial Catalog=prototype; Integrated Security=SSPI;";
        SqlCommand command;
        SqlDataReader dataReader;




   
        /// <summary>
        /// Method for getting the User Statistics data
        /// </summary>
        /// <param name="questions"></param>
        /// <param name="count"></param>
        public void UserStats(out List<string> questions, out List<string> count)
        {
            Connect();
            sql = "SELECT name, COUNT(question) FROM user_question AS uq JOIN [user] AS u ON (uq.user_id= u.id) JOIN [question] AS q ON (uq.question_id = q.id) GROUP BY name";
            questions = new List<string>(Get(sql, 0));
            count = new List<string>(Get(sql, 1));
           
            DisConnect();
        }
        /// <summary>
        /// Method for getting the System Statistics data
        /// </summary>
        /// <param name="questions"></param>
        /// <param name="count"></param>
        public void SystemStats(out List<string> questions, out List<string> count)
        {
            Connect();
            sql = "SELECT question, COUNT(name) FROM user_question AS uq JOIN [user] AS u ON (uq.user_id= u.id) JOIN [question] AS q ON (uq.question_id = q.id) GROUP BY question";
            questions = new List<string>(Get(sql, 0));
            count = new List<string>(Get(sql, 1));

            DisConnect();
        }


        /// <summary>
        /// Connect to the database with the connectionString
        /// </summary>
        public void Connect()
        {
            

            cnn = new SqlConnection(connectionString);
            cnn.Open();
        }
       
        /// <summary>
        /// Execute the query from the string sql
        /// </summary>
        /// <param name="sql"></param>
        public void Execute(string sql)
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
        /// Disconnect to the database and dispose of the command
        /// </summary>
        public void DisConnect()
        {
            command.Dispose();
            cnn.Close();
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
        /// Insert the question, clear the lists lastVerbs and lastNouns. the lists are for checking if they already exist,
        /// The variable is analyzed for the verb and noun, the table and column tell where the question needs to be inserted
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
                lastQuestion = Get("SELECT id FROM[" + table + "] WHERE " + column + " = '" + variable + "'", 0).First();
                Connect();

                bool numericAnswer = int.TryParse(lastAnswer, out int lastAnswerId);
                string answer = (numericAnswer) ? "'" + lastAnswer + "'" : "(SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "')";
                string question =  "'" + lastQuestion + "'";
                sql = $"IF NOT EXISTS (SELECT * FROM keysentence WHERE question_id = {question} AND answer_id = {answer}) BEGIN INSERT INTO keysentence(question_id, answer_id) VALUES({question}, {answer}) END";
                Execute(sql);
            }

            DisConnect();
        }


        //Calls the Google NLP Api and throws the response into writeEntitySentiment function
        protected virtual void AnalyzeIncomingData(string text)
        {
            var client = LanguageServiceClient.Create();

            var response = client.AnalyzeSyntax(new Document()
            {
                Content = text,
                Type = Document.Types.Type.PlainText

            });

            this.WriteEntitySentiment(response.Tokens);
        }

        //Fills the Sentiments array with the entities values
        protected virtual void WriteEntitySentiment(RepeatedField<Token> tokens)
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


            insertRelations();
         
       
            
        }

        /// <summary>
        /// Insert the data necessary for all relations to work in the database
        /// </summary>
        public void insertRelations()
        {
           
            
            bool numericQuestion = int.TryParse(lastQuestion, out int lastQuestionId);
            bool numericAnswer = int.TryParse(lastAnswer, out int lastAnswerId);
            string answer = (numericAnswer) ? "'" + lastAnswer + "'" : "(SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "')";
            string question = (numericQuestion) ? "'" + lastQuestion + "'" : "(SELECT TOP 1 id FROM question WHERE question = '" + lastQuestion + "')";
            sql = $"IF NOT EXISTS (SELECT * FROM keysentence WHERE question_id = {question} AND answer_id = {answer}) BEGIN INSERT INTO keysentence(question_id, answer_id) VALUES({question}, {answer}) END";
            Execute(sql);


            // Insert noun_keysentence and verb_keysentence for each instance of a verb (many to many relationship)
            foreach (var verb in lastVerbs)
            {
                // Check if they are id's or text, if they are id's, insert them directly, else search for the verb where the word = verb
                bool numericVerb = int.TryParse(verb, out int lastVerbId);
                string check =
                    $"IF NOT EXISTS (SELECT * FROM verb_keysentence WHERE verb_id = {verb} AND keysentence_id = (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '{lastAnswer}'))) BEGIN";
                string checkNumeric =
                    $"IF NOT EXISTS (SELECT * FROM verb_keysentence WHERE verb_id = (SELECT TOP 1 id FROM verb WHERE word = '{verb}') AND keysentence_id = (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '{lastAnswer}'))) BEGIN";
                sql = (numericVerb) ? $"{check} INSERT INTO verb_keysentence(verb_id, keysentence_id) VALUES('" + verb + "', (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "'))) END" : $"{checkNumeric} INSERT INTO verb_keysentence(verb_id, keysentence_id) VALUES( (SELECT TOP 1 id FROM verb WHERE word = '" + verb + "'), (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "'))) END";

                Execute(sql);
            }

            foreach (var noun in lastNouns)
            {
                // Same goes for here, except now for the noun
                bool numericNoun = int.TryParse(noun, out int lastNounId);
                string check =
                    $"IF NOT EXISTS (SELECT * FROM noun_keysentence WHERE noun_id = {noun} AND keysentence_id = (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '{lastAnswer}'))) BEGIN";
                string checkNumeric =
                    $"IF NOT EXISTS (SELECT * FROM noun_keysentence WHERE noun_id = (SELECT TOP 1 id FROM noun WHERE word = '{noun}') AND keysentence_id = (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '{lastAnswer}'))) BEGIN";
                sql = (numericNoun) ? $"{check} INSERT INTO noun_keysentence(noun_id, keysentence_id) VALUES('" + noun + "', (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "'))) END" : $"{checkNumeric} INSERT INTO noun_keysentence(noun_id, keysentence_id) VALUES( (SELECT TOP 1 id FROM noun WHERE word = '" + noun + "'), (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "'))) END";

                Execute(sql);
            }
           

        }

    }

}