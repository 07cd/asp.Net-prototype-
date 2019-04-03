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
    public class Model : Database
    {
        
        static List<string> allNouns;
        static List<string> allVerbs;
        static List<string> allAnswers;
        static List<string> allQuestions;
      
        static List<string> lastVerbs = new List<string>();
        static List<string> lastNouns = new List<string>();
       
        static public string lastAnswer = "";
       
        static public string lastQuestion = "";
       
        static string sql = "";
        

   



        /// <summary>
        /// Method inserts the answer into the database, if it exists get the id.
        /// LastAnswer is either set to the incoming variable or to the id,
        /// this will be checked as to insert it directly or search the id where the variable is.
        /// </summary>
        //Insert the answer, or if the answer already exists get id
        public static void Insert(List<string> answer, string variable, string table, string column)
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
      
        public static void InsertQuestion(string Answer, List<string> questions, List<string> nouns, List<string> verbs, string variable, string table, string column)
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
                NLP.AnalyzeIncomingData(variable, allNouns, allVerbs, allQuestions );

                var verb = NLP.lastVerbs;
                var test = NLP.lastNouns;


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



        /// <summary>
        /// Insert the data necessary for all relations to work in the database
        /// </summary>
        public static void insertRelations()
        {
           
            
            bool numericQuestion = int.TryParse(lastQuestion, out int lastQuestionId);
            bool numericAnswer = int.TryParse(lastAnswer, out int lastAnswerId);
            string answer = (numericAnswer) ? "'" + lastAnswer + "'" : "(SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "')";
            string question = (numericQuestion) ? "'" + lastQuestion + "'" : "(SELECT TOP 1 id FROM question WHERE question = '" + lastQuestion + "')";
            sql = $"IF NOT EXISTS (SELECT * FROM keysentence WHERE question_id = {question} AND answer_id = {answer}) BEGIN INSERT INTO keysentence(question_id, answer_id) VALUES({question}, {answer}) END";
            Execute(sql);

        
            // Insert noun_keysentence and verb_keysentence for each instance of a verb (many to many relationship)
            foreach (var verb in NLP.lastVerbs)
            {
                Debug.WriteLine(verb);
                // Check if they are id's or text, if they are id's, insert them directly, else search for the verb where the word = verb
                bool numericVerb = int.TryParse(verb, out int lastVerbId);
                string check =
                    $"IF NOT EXISTS (SELECT * FROM verb_keysentence WHERE verb_id = {verb} AND keysentence_id = (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '{lastAnswer}'))) BEGIN";
                string checkNumeric =
                    $"IF NOT EXISTS (SELECT * FROM verb_keysentence WHERE verb_id = (SELECT TOP 1 id FROM verb WHERE word = '{verb}') AND keysentence_id = (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '{lastAnswer}'))) BEGIN";
                sql = (numericVerb) ? $"{check} INSERT INTO verb_keysentence(verb_id, keysentence_id) VALUES('" + verb + "', (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "'))) END" : $"{checkNumeric} INSERT INTO verb_keysentence(verb_id, keysentence_id) VALUES( (SELECT TOP 1 id FROM verb WHERE word = '" + verb + "'), (SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '" + lastAnswer + "'))) END";

                Execute(sql);
            }

            foreach (var noun in NLP.lastNouns)
            {
                Debug.WriteLine(noun);
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

        public static void linkKeywords(List<string> nounList, List<string> verbList, string answer)
        {
            
            Connect();

            
            
            string question = "";

            nounList.ForEach(delegate(string noun) { question += noun; });
            verbList.ForEach(delegate (string verb) { question += verb; });

            sql = $"INSERT INTO question(question) VALUES('{question}')";
            Execute(sql);


            sql = $"INSERT INTO keysentence(question_id, answer_id) VALUES((SELECT TOP 1 id FROM question WHERE question = '{question}'), (SELECT TOP 1 id FROM answer WHERE answer = '{answer}'))";
            Execute(sql);

            nounList.ForEach(delegate(string noun)
                {
                    sql =
                        $"INSERT INTO noun_keysentence(noun_id, keysentence_id) VALUES((SELECT TOP 1 id FROM noun WHERE word = '{noun}'),(SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '{answer}') AND question_id = (SELECT TOP 1 id FROM question WHERE question = '{question}')))";
                    Execute(sql);
                });
            verbList.ForEach(delegate (string verb)
            {
                sql =
                    $"INSERT INTO verb_keysentence(verb_id, keysentence_id) VALUES((SELECT TOP 1 id FROM verb WHERE word = '{verb}'),(SELECT TOP 1 id FROM keysentence WHERE answer_id = (SELECT TOP 1 id FROM answer WHERE answer = '{answer}') AND question_id = (SELECT TOP 1 id FROM question WHERE question = '{question}')))";
                Execute(sql);
            });

            
            
            DisConnect();
        }

        public static void InserWord(string word, string type)
        {
            Connect();
            sql = "INSERT INTO "+ ((type == "noun")? "noun" : "verb") + $"(word) VALUES('{word}')";
            Execute(sql);
            DisConnect();
        }
    }

}