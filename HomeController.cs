using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;

namespace LinkedinApiTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            try
            {

                Response.Redirect("https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id=81slso60hk1i1q&redirect_uri=http://localhost:5784/Home/Callback&state=987654321&scope=r_basicprofile");
                //using (HttpClient client = new HttpClient())
                //{
                //    client.BaseAddress = new Uri(uri);
                //    client.DefaultRequestHeaders.Accept.Clear();
                //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                //    client.Timeout = TimeSpan.FromMinutes(30);
                //    HttpResponseMessage response = client.GetAsync(uri).Result;
                //}

            }
            catch (Exception ex)
            {
            }
            return View();
        }

       

        public ActionResult Callback(string code,string access_token)
        {
            if (code != null)
            {
                Response.Redirect("https://www.linkedin.com/oauth/v2/accessToken?grant_type=authorization_code&code=" + code + "&client_id=81slso60hk1i1q&client_secret=RtoykmzOYHHSMk7O&redirect_uri=http://localhost:5784/Home/Callback");

            }
            else if(access_token != null)
            {
            }
            
            return View();
        }
    }
}