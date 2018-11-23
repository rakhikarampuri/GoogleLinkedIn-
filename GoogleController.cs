using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace LinkedinApiTest.Controllers
{
    public class GoogleController : Controller
    {
        // GET: Google
        string clientid = "362547684656-3uvmrt9ochhlverpj6uap1vil8pr2vvb.apps.googleusercontent.com";
        //your client secret  
        string clientsecret = "DqBo2xYOK-E-CgFSmZBfUgpf";
        //your redirection url  
        string redirection_url = "http://localhost:5784/google/callback";
        //string redirection_url = "http://linkedintest.com/google/callback";
        string url = "https://accounts.google.com/o/oauth2/token";
        public ActionResult Index()
        {
            try
            {
                string Url = "https://accounts.google.com/o/oauth2/auth?scope={0}&redirect_uri={1}&response_type={2}&client_id={3}&state={4}&access_type={5}&approval_prompt={6}";
                string scope = _urlEncodeForGoogle("https://mail.google.com/ https://www.googleapis.com/auth/gmail.readonly https://www.googleapis.com/auth/gmail.modify https://www.googleapis.com/auth/gmail.labels https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/gmail.settings.basic https://www.googleapis.com/auth/gmail.compose https://www.googleapis.com/auth/gmail.send https://www.googleapis.com/auth/gmail.insert https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/calendar.readonly").Replace("%20", "+");
                string redirect_uri_encode = _urlEncodeForGoogle(redirection_url);
                string response_type = "code";
                //string state = "profile";
                string state = "";
                state = _urlEncodeForGoogle(state);
                string access_type = "offline";
                string approval_prompt = "force";


                var ddd =string.Format(Url,
                                scope,
                                redirect_uri_encode,
                                response_type,
                                clientid,
                                state,
                                access_type,
                                approval_prompt);
                string url = "https://accounts.google.com/o/oauth2/v2/auth?scope=profile&include_granted_scopes=true&redirect_uri=" + redirection_url + "&response_type=code&client_id=" + clientid + "";
                Response.Redirect(ddd);
            }
            catch (Exception ex)
            {

                throw;
            }
            return View();
        }

      
        public ActionResult callback(string code, string accesstoken)
        {
            List<Models.modeluser> modeluser = new List<Models.modeluser>();
            var results = GetToken(code);
          
            return View();
        }

        public UserInfo GetToken(string code)
        {
            string poststring = "grant_type=authorization_code&code=" + code + "&client_id=" + clientid + "&client_secret=" + clientsecret + "&redirect_uri=" + redirection_url + "";
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            UTF8Encoding utfenc = new UTF8Encoding();
            byte[] bytes = utfenc.GetBytes(poststring);
            Stream outputstream = null;
            try
            {
                request.ContentLength = bytes.Length;
                outputstream = request.GetRequestStream();
                outputstream.Write(bytes, 0, bytes.Length);
            }
            catch { }
            var response = (HttpWebResponse)request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            string responseFromServer = streamReader.ReadToEnd();
            JavaScriptSerializer js = new JavaScriptSerializer();
            Tokenclass obj = js.Deserialize<Tokenclass>(responseFromServer);
            var results = GetAllProfileData(obj.access_token);
            return results;
        }

        public UserInfo GetAllProfileData(string accesstoken)
        {
            UserInfo objInfo = null;

            string strUserInfoUri = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json";
            var userInfo_httpWebRequest = HttpWebRequest.Create(strUserInfoUri) as HttpWebRequest;
            userInfo_httpWebRequest.CookieContainer = new CookieContainer();
            userInfo_httpWebRequest.Headers["Authorization"] = string.Format("Bearer {0}", accesstoken);
            var userInfo_response = userInfo_httpWebRequest.GetResponse();
            Stream userInfo_receiveStream = userInfo_response.GetResponseStream();
            StreamReader userInfo_readStream = new StreamReader(userInfo_receiveStream, Encoding.UTF8);
            var userInfo_results = userInfo_readStream.ReadToEnd();
            userInfo_receiveStream.Close();
            userInfo_receiveStream.Dispose();
            userInfo_readStream.Close();
            userInfo_readStream.Dispose();
            GoogleUserInfo objUserInfo = JsonConvert.DeserializeObject<GoogleUserInfo>(userInfo_results);

            // User email call
            string strProfileUri = "https://www.googleapis.com/gmail/v1/users/" + "me" + "/profile";
            GoogleUserEmailInfo objUserEmailInfo = new GoogleUserEmailInfo();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(strProfileUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.GetAsync(strProfileUri).Result;
                if (response.IsSuccessStatusCode)
                    objUserEmailInfo = response.Content.ReadAsAsync<GoogleUserEmailInfo>().Result;
            }
            objInfo = new UserInfo()
            {
                Email = objUserEmailInfo.emailAddress,
                Firstname = objUserInfo.given_name,
                Gender = objUserInfo.gender,
                Lastname = objUserInfo.family_name,
                Link = objUserInfo.link,
                Locale = objUserInfo.locale,
                Name = objUserInfo.name,
                Picture = objUserInfo.picture
            };
            return objInfo;

        }

        #region Encrypt   
        private string _urlEncodeForGoogle(string url)
        {
            string UnReservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
            var result = new StringBuilder();

            foreach (char symbol in url)
            {
                if (UnReservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }
            return result.ToString();
        }
        #endregion

        #region Classes
        public class UserInfo
        {
            public string ID { get; set; }
            public string Name { get; set; }
            public string Firstname { get; set; }
            public string Lastname { get; set; }
            public string Link { get; set; }
            public string Picture { get; set; }
            public string Gender { get; set; }
            public string Locale { get; set; }
            public string Email { get; set; }
        }

        public class Tokenclass
        {
            public string access_token
            {
                get;
                set;
            }
            public string token_type
            {
                get;
                set;
            }
            public int expires_in
            {
                get;
                set;
            }
            public string refresh_token
            {
                get;
                set;
            }
        }

        public class GoogleUserInfo
        {
            public string id { get; set; }
            public string name { get; set; }
            public string given_name { get; set; }
            public string family_name { get; set; }
            public string link { get; set; }
            public string picture { get; set; }
            public string gender { get; set; }
            public string locale { get; set; }
        }

        public class GoogleUserEmailInfo
        {
            public string emailAddress { get; set; }
            public string messagesTotal { get; set; }
            public string threadsTotal { get; set; }
            public string historyId { get; set; }
        }
        #endregion

    }
}