using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using Google.Cloud.Language.V1;
using Google.Protobuf.Collections;
using Newtonsoft.Json;

namespace dotNetTest.Models
{
    public class Find : Model
    {
        public string Output;
        private static List<string> sentiments = new List<string>();
        private string noun;
        public string verb;
        public string strOutput;
        private static List<string> allNouns = new List<string>();
        private static List<string> allVerbs = new List<string>();
        public Find(string input, out string output)
        {
            Debug.WriteLine(input);
            string credential_path = @"D:\dotNetTest\dotNetTest\Adriaan-18cad82b0123.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
            allNouns.Clear();
            allVerbs.Clear();
            this.AnalyzeIncomingData(input);
            Debug.WriteLine(strOutput);
            output = strOutput;
        }

      
        protected void AnalyzeIncomingData(string text)
        {
            var client = LanguageServiceClient.Create();
            Debug.WriteLine(text);
            var response = client.AnalyzeSyntax(new Document()
            {
                Content = text,
                Type = Document.Types.Type.PlainText

            });

            this.WriteEntitySentiment(response.Tokens);
        }

        protected void WriteEntitySentiment(RepeatedField<Token> tokens)
        {
            dynamic jsonObj = JsonConvert.DeserializeObject(tokens.ToString());

            foreach (var obj in jsonObj)
            {


                if (obj.partOfSpeech.tag == "VERB" || obj.partOfSpeech.tag == "NOUN")
                {
                    sentiments.Add(obj.text.content.ToString());
                    if (obj.partOfSpeech.tag == "NOUN")
                    {
                        allNouns.Add(obj.text.content.ToString());
                         noun = obj.text.content.ToString();
                    }
                    if (obj.partOfSpeech.tag == "VERB")
                    {
                        allVerbs.Add(obj.text.content.ToString());
                        verb = obj.text.content.ToString();
                    }
                }
            }

            Connect();
            
            try
            {
               
                StringBuilder sql = new StringBuilder();
                sql.Append($"SELECT answer FROM keysentence AS ks JOIN verb_keysentence AS vk ON vk.keysentence_id = ks.id JOIN noun_keysentence AS nk ON nk.keysentence_id = ks.id JOIN verb AS v ON v.id = verb_id JOIN noun AS n ON n.id = noun_id JOIN answer AS a ON a.id = ks.answer_id WHERE");
                foreach (var verb in allVerbs)
                {
                    if (allVerbs.IndexOf(verb) < allVerbs.Count - 1 && allVerbs.Count > 1)
                    {
                        sql.Append($" v.word = '{verb}' AND");
                    }
                    else
                    {
                        sql.Append($" v.word = '{verb}'");
                    }

                }
                foreach (var noun in allNouns)
                {
                    sql.Append($" AND n.word = '{noun}'");
                }
                Debug.WriteLine(sql.ToString());
                Output = Get(sql.ToString(), 0).First();
                strOutput = Output;
            }
            catch{ }
            DisConnect();
                


        }


    }
}