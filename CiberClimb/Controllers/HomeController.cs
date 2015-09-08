using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CiberClimb.Models;
using CiberClimb.Services;
using Newtonsoft.Json;

namespace CiberClimb.Controllers
{
    public class HomeController : Controller
    {
        public async Task<ActionResult> Index()
        {
            ClimberService cs = new ClimberService();
            var climberList = await cs.GetClimberModels();
            return View(climberList);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            ////string urlWithAccessToken = "https://cibernordic.slack.com/services/hooks/incoming-webhook?token={your_access_token}";
            //string urlWithAccessToken = "https://hooks.slack.com/services/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";

            //SlackClient client = new SlackClient(urlWithAccessToken);

            //client.PostMessage(username: "Mr. Torgue",
            //                       text: "THIS IS A TEST MESSAGE! SQUEEDLYBAMBLYFEEDLYMEEDLYMOWWWWWWWW!",
            //                    channel: "#bot-test");

            return View();
        }
    }

    //A simple C# class to post messages to a Slack channel
    //Note: This class uses the Newtonsoft Json.NET serializer available via NuGet
    public class SlackClient
    {
        private readonly Uri _uri;
        private readonly Encoding _encoding = new UTF8Encoding();

        public SlackClient(string urlWithAccessToken)
        {
            _uri = new Uri(urlWithAccessToken);
        }

        //Post a message using simple strings
        public void PostMessage(string text, string username = null, string channel = null)
        {
            Payload payload = new Payload()
            {
                Channel = channel,
                Username = username,
                Text = text
            };

            PostMessage(payload);
        }

        //Post a message using a Payload object
        public void PostMessage(Payload payload)
        {
            string payloadJson = JsonConvert.SerializeObject(payload);

            using (WebClient client = new WebClient())
            {
                NameValueCollection data = new NameValueCollection();
                data["payload"] = payloadJson;

                var response = client.UploadValues(_uri, "POST", data);

                //The response text is usually "ok"
                string responseText = _encoding.GetString(response);
            }
        }
    }

    //This class serializes into the Json payload required by Slack Incoming WebHooks
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}