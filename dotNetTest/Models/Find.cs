﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
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
        public Find(string input, out string output)
        {
            Debug.WriteLine(input);
            string credential_path = @"D:\dotNetTest\dotNetTest\Adriaan-18cad82b0123.json";
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);
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
                         noun = obj.text.content.ToString();
                    }
                    if (obj.partOfSpeech.tag == "VERB")
                    {
                        verb = obj.text.content.ToString();
                    }
                }
            }

            Connect();
            //Todo: possible to have more than 1 keyword and 1 noun
            try
            {
                string sql =
                    $"SELECT answer FROM keysentence AS ks JOIN verb_keysentence AS vk ON vk.keysentence_id = ks.id JOIN noun_keysentence AS nk ON nk.keysentence_id = ks.id JOIN verb AS v ON v.id = verb_id JOIN noun AS n ON n.id = noun_id JOIN answer AS a ON a.id = ks.answer_id WHERE v.word = '{verb}' AND n.word = '{noun}'";

                Output = Get(sql, 0).First().ToString();
                strOutput = Output;
            }
            catch{ }
            DisConnect();
                


        }


    }
}