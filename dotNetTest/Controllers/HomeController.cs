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
            //Day
            Stats.TimeStats(out List<string[]> dData, "SELECT count(*), [date] FROM user_question GROUP BY[date]");
            ViewData["DayData"] = dData;
            ViewData["MonthData"] = mData;
            ViewData["YearData"] = yData;

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
        public ActionResult UserStats()
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Find()
        {
            Debug.WriteLine(Request.Form["answer"]);
            new Find(Request.Form["answer"], out string Output);   

            TempData.Add("answer", Output);
            return (RedirectToAction("Index", new { answer = Output }));
            
        }
        [HttpPost]
        public ActionResult Remove()
        {
            Debug.WriteLine(Request.Form["remove"] +""+ Request.Form["type"]);
            new Remove(Request.Form["remove"], Request.Form["type"] );
            return RedirectToAction("Index");
        }

       

    }
}