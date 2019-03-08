using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace dotNetTest.Models
{
    public class User : Model
    {
        //Constructor
        public User(int id, string name)
        {
            
            //Insert user in database
        }
        public User()
        {
            //Do nothing
        }
        int id { get; set; }
        string name { get; set; }



    }
}