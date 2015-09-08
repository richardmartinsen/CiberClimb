using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Schema;
using CiberClimb.Services;
using Newtonsoft.Json;

namespace CiberClimbApi.Controllers
{
    using CiberClimb.Models;

    public class ClimbController : ApiController
    {
        [HttpPost]
        public async Task<string> Post()
        {
            string urlWithAccessToken = "https://hooks.slack.com/services/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";

            SlackClient client = new SlackClient(urlWithAccessToken);

            ClimberService cs = new ClimberService();
            var climbers = await cs.GetClimberModels();
            //var climbers = this.GetClimberModels();
            var tekst = string.Empty;

            tekst += "```";
            tekst += String.Format("{0,-10} {1,-20} {2,-10} {3,-10} {4,-10} {5,-10}\n", "Plass", "Navn", "Kongsveien", "Grefsen", "Tryvann", "Total");
            foreach (var rider in climbers)
            {
                var kongsveien = rider.KongsveienPoints + "(" + rider.KongsveienTime + ")";
                var grefsen = rider.GrefsenPoint + "(" + rider.GrefsenTime + ")";
                var tryvann = rider.TryvannPoints + "(" + rider.TryvannTime + ")";
                tekst += String.Format("{0,-10} {1,-20} {2,-10} {3,-10} {4,-10} {5,-10}\n", rider.Place, rider.Name, kongsveien, grefsen, tryvann, rider.TotalPoints);
            }
            tekst += "```";

            client.PostMessage(username: "Sykkelbot",
                                   text: tekst,
                                channel: "#sykkelgruppa");
            return tekst;
        }
    }

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
}
