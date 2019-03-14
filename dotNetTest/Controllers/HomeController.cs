using dotNetTest.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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



        public ActionResult Index()
        {

            //Response.Write(test.Get("SELECT * FROM [user]", 1));
            ViewData["Users"] = users.Get("SELECT * FROM [user]", 1);
            ViewData["Nouns"] = nouns.Get("SELECT * FROM [noun]", 1);
            ViewData["Verbs"] = verbs.Get("SELECT * FROM [verb]", 1);
            ViewData["Questions"] = questions.Get("SELECT * FROM [question]", 1);
            ViewData["Answers"] = answers.Get("SELECT * FROM [answer]", 1);
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
    }
}