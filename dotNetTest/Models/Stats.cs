using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotNetTest.Models
{
    public class Stats : Model
    {
        private static string sql;
        /// <summary>
        /// Method for getting the User Statistics data
        /// </summary>
        /// <param name="questions"></param>
        /// <param name="count"></param>
        public static void UserStats(out List<string> questions, out List<string> count)
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
        public static void SystemStats(out List<string> questions, out List<string> count)
        {
            Connect();
            sql = "SELECT question, COUNT(name) FROM user_question AS uq JOIN [user] AS u ON (uq.user_id= u.id) JOIN [question] AS q ON (uq.question_id = q.id) GROUP BY question";
            questions = new List<string>(Get(sql, 0));
            count = new List<string>(Get(sql, 1));

            DisConnect();
        }

        public static void TimeStats(out List<string[]> timeStats, string query)
        {
            Connect();
            sql = query;
           
            List<string> count = new List<string>(Get(sql, 0));
            List<string> time = new List<string>(Get(sql, 1));
            timeStats = new List<string[]>();
            for (int i = 0; i < count.Count; i++)
            {
                
                string[] test = new[] {count[i], time[i]};
                timeStats.Add(test);
            }
            DisConnect();

        }

    }
}