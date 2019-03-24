using dotNetTest.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace dotNetTest.Controllers
{
    public class HomeController : Controller
    {
        
      
        public ActionResult Index(string answer)
        {
            ViewData["Users"] = Database.Get("SELECT * FROM [user]", 1);
            ViewData["Nouns"] = Database.Get("SELECT * FROM [noun]", 1);
            ViewData["Verbs"] = Database.Get("SELECT * FROM [verb]", 1);
            ViewData["Questions"] = Database.Get("SELECT * FROM [question]", 1);
            ViewData["Answers"] = Database.Get("SELECT * FROM [answer]", 1);
            //Year
            Stats.TimeStats(out List<string[]> yData, "SELECT count(*), DATEPART(year, [date]) FROM user_question GROUP BY DATEPART(year, [date])");
            //Month
            Stats.TimeStats(out List<string[]> mData, "SELECT count(*), FORMAT([date], 'Y') FROM user_question GROUP BY  FORMAT([date], 'Y')");
            //Week
            Stats.TimeStats(out List<string[]> wData, "SELECT count(*), DATEPART(wk, [date]) FROM user_question GROUP BY  DATEPART(wk, [date])");
            //Day
            Stats.TimeStats(out List<string[]> dData, "DECLARE @StartDate datetime = CONVERT(varchar(10),YEAR(GetDate())) DECLARE @EndDate datetime = SYSDATETIME() ; WITH days AS(SELECT DATEADD(DAY, n, DATEADD(DAY, DATEDIFF(DAY, 0, @StartDate), 0)) as d FROM(SELECT TOP(DATEDIFF(DAY, @StartDate, @EndDate) + 1) n = ROW_NUMBER() OVER(ORDER BY[object_id]) - 1 FROM sys.all_objects ORDER BY[object_id]) AS n) select count(t.user_id), CONVERT(date, days.d) FROM days LEFT OUTER JOIN user_question as t ON t.[date]>= days.d AND t.[date] < DATEADD(DAY, 1, days.d) Where d BETWEEN DATEADD(DAY, -7, GETDATE()) AND DATEADD(DAY, 1, GETDATE()) GROUP BY days.d ORDER BY days.d;");
            ViewData["DayData"] = dData;
            ViewData["MonthData"] = mData;
            ViewData["WeekData"] = wData;
            ViewData["YearData"] = yData;
            Stats.UserStats(out List<string> users, out List<string> userCount, out List<string[]> userStatsList);
            ViewData["UserStat"] = users;
            ViewData["UserStatCount"] = userCount;
            ViewData["UserStatList"] = userStatsList;
            Stats.SystemStats(out List<string> questions, out List<string> questionCount , out List<string[]> systemStatsList);
            ViewData["SystemStat"] = questions;
            ViewData["SystemStatCount"] = questionCount;
            ViewData["SystemStatList"] = systemStatsList;

          

            return View();
            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult ChartArrayBasic()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
       
        /// <summary>
        /// Post controller for inserting the question and answer
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Insert()
        {
            var allQuestions = Database.Get("SELECT * FROM [question]", 1);
            var allAnswers = Database.Get("SELECT * FROM [answer]", 1);
            List<string> allNouns = Database.Get("SELECT * FROM [noun]", 1);
            List<string> allVerbs = Database.Get("SELECT * FROM [verb]", 1);

            var answer = Request.Form["answer"];
            var question = Request.Form["question"];

      
            Model.Insert(allAnswers, answer, "answer", "answer");
            
            Model.InsertQuestion(answer, allQuestions, allNouns, allVerbs, question, "question", "question");
            return RedirectToAction("About");
        }

        [HttpPost]
        public ActionResult Find()
        {
            Debug.WriteLine(Request.Form["answer"]);
            new Find(Request.Form["answer"], out string Output);   

            TempData.Add("answer", Output);
            return (RedirectToAction("About", new { answer = Output }));
            
        }
        [HttpPost]
        public ActionResult Remove()
        {
            Debug.WriteLine(Request.Form["remove"] +""+ Request.Form["type"]);
            new Remove(Request.Form["remove"], Request.Form["type"] );
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UserStats(string user)
        {
            string amountToday = Database.Get($"SELECT count(*) FROM user_question JOIN [user] AS u ON user_question.user_id = u.id  WHERE date = (CONVERT (date, GETDATE())) AND name = '{user}'", 0).First().ToString();
            string amountAll = Database
                .Get(
                    $"SELECT count(*) FROM user_question JOIN [user] AS u ON user_question.user_id = u.id WHERE name = '{user}'",
                    0).First().ToString();


            //Year
            Stats.TimeStats(out List<string[]> yData, $"SELECT count(name), DATEPART(year, [date]) FROM user_question JOIN [user] AS u ON user_question.user_id = u.id WHERE name = '{user}' GROUP BY DATEPART(year, [date])");
            //Month
            Stats.TimeStats(out List<string[]> mData, $"SELECT count(name),  FORMAT([date], 'Y') FROM user_question JOIN [user] AS u ON user_question.user_id = u.id WHERE name = '{user}' GROUP BY FORMAT([date], 'Y')");
            //Week
            Stats.TimeStats(out List<string[]> wData, "SELECT count(name), DATEPART(wk, [date]) FROM user_question JOIN [user] AS u ON user_question.user_id = u.id WHERE name = '{user}' GROUP BY DATEPART(wk, [date])");
            //Day
            Stats.TimeStats(out List<string[]> dData, $"SELECT count(name), date FROM user_question JOIN [user] AS u ON user_question.user_id = u.id WHERE name = '{user}' GROUP BY date");

            TempData.Add("UserYearData", yData);
            TempData.Add("UserMonthData", mData);
            TempData.Add("UserWeekData", wData);
            TempData.Add("UserDayData", dData);
            TempData.Add("UserAmountToday", amountToday.First());
            TempData.Add("UserAmountAll", amountAll.First());



            return (RedirectToAction("Index", new { userAmountAll = amountAll, userAmountToday = amountToday, userDayData = dData, userWeekData = wData,  userMonthData = mData, userYearData = yData, }));
        }

        [HttpPost]
        public ActionResult QuestionStats(string question)
        {
            
            
         
            string amountToday = Database.Get($"SELECT count(question) FROM user_question JOIN question ON user_question.question_id = question.id WHERE date = (CONVERT (date, GETDATE())) AND question = '{question}'", 0).First().ToString();
            string amountAll = Database.Get($"SELECT count(question) FROM user_question JOIN question ON user_question.question_id = question.id WHERE question = '{question}'", 0).First().ToString();


            //Year
            Stats.TimeStats(out List<string[]> yData, $"SELECT count(question), DATEPART(year, [date]) FROM user_question JOIN question ON user_question.question_id = question.id WHERE question = '{question}' GROUP BY DATEPART(year, [date])");
            //Month
            Stats.TimeStats(out List<string[]> mData, $"SELECT count(question),  FORMAT([date], 'Y') FROM user_question JOIN question ON user_question.question_id = question.id WHERE question = '{question}' GROUP BY FORMAT([date], 'Y')");
            //Day
            Stats.TimeStats(out List<string[]> dData, $"SELECT count(question), date FROM user_question JOIN question ON user_question.question_id = question.id WHERE question = '{question}' GROUP BY date");
            
            TempData.Add("QuestionYearData", yData);
            TempData.Add("QuestionMonthData", mData);
            TempData.Add("QuestionDayData", dData);
            TempData.Add("QuestionAmountToday", amountToday);
            TempData.Add("questionAmountAll", amountAll);

            
            
            return (RedirectToAction("Index", new { questionAmountAll = amountAll, questionAmountToday = amountToday, questionDayData = dData,  questionMonthData = mData, questionYearData = yData,  }));
        }

    }
}