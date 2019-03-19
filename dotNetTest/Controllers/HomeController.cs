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
        User users = new User();
        Noun nouns = new Noun();
        Verb verbs = new Verb();
        Answer answers = new Answer();
        Question questions = new Question();



        public ActionResult Index(string answer)
        {
            ViewData["Users"] = users.Get("SELECT * FROM [user]", 1);
            ViewData["Nouns"] = nouns.Get("SELECT * FROM [noun]", 1);
            ViewData["Verbs"] = verbs.Get("SELECT * FROM [verb]", 1);
            ViewData["Questions"] = questions.Get("SELECT * FROM [question]", 1);
            ViewData["Answers"] = answers.Get("SELECT * FROM [answer]", 1);
            //Response.Write(test.Get("SELECT * FROM [user]", 1));

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
            var allQuestions = questions.Get("SELECT * FROM [question]", 1);
            var allAnswers = answers.Get("SELECT * FROM [answer]", 1);
            List<string> allNouns = nouns.Get("SELECT * FROM [noun]", 1);
            List<string> allVerbs = verbs.Get("SELECT * FROM [verb]", 1);

            var answer = Request.Form["answer"];
            var question = Request.Form["question"];

            Answer answerInsert = new Answer();
            answerInsert.Insert(allAnswers, answer, "answer", "answer");
            Question questionInsert = new Question();
            questionInsert.InsertQuestion(answer, allQuestions, allNouns, allVerbs, question, "question", "question");
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