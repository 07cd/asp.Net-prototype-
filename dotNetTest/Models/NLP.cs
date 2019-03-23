using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

using Google.Protobuf.Collections;
using Google.Cloud.Language.V1;
using System.Diagnostics;


namespace dotNetTest.Models
{
    public class NLP : Database
    {

        Model dataBase = new Model();
        static List<string> allNouns;
        static List<string> allVerbs;
      
      
        public static List<string> lastVerbs = new List<string>();
        public static List<string> lastNouns = new List<string>();

    
        static string sql = "";
        static private List<string> sentiments = new List<string>();


        //Calls the Google NLP Api and throws the response into writeEntitySentiment function
        public static void AnalyzeIncomingData(string text, List<string> nouns, List<string> verbs, List<string> questions)
        {
            var client = LanguageServiceClient.Create();

            var response = client.AnalyzeSyntax(new Document()
            {
                Content = text,
                Type = Document.Types.Type.PlainText

            });


            allNouns = nouns;
            allVerbs = verbs;


            WriteEntitySentiment(response.Tokens);
        }

        //Fills the Sentiments array with the entities values
        protected static void WriteEntitySentiment(RepeatedField<Token> tokens)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(tokens.ToString());

          
            foreach (var obj in jsonObj)
            {


                if (obj.partOfSpeech.tag == "VERB" || obj.partOfSpeech.tag == "NOUN")
                {
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


            Model.insertRelations();



        }
    }
}