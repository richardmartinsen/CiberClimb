using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Xml.Schema;
using Newtonsoft.Json;

namespace CiberClimbApi.Controllers
{
    using CiberClimb.Models;

    public class ClimbController : ApiController
    {
        [HttpPost]
        public string Post()
        {
            string urlWithAccessToken = "https://hooks.slack.com/services/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";

            SlackClient client = new SlackClient(urlWithAccessToken);

            var climbers = this.GetClimberModels(1);
            for (int i = 1; i < 3; i++)
            {
                climbers.AddRange(this.GetClimberModels(i));
            }
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

        public List<ClimberModels> GetClimberModels(int page)
        {
            const string url = "http://www.klatrekonge.com/herrer-oslo?page=" + page;

            var climberList = new List<ClimberModels>();
            using (var client = new HttpClient())
            {
                var ciberNames = new[] { "Mathias Moen", "Richard Martinsen", "Joakim Bjerkheim", "Kyrre Havik Eriksen" };

                try
                {
                    var result = client.GetStreamAsync(url).Result;
                    var doc = new HtmlAgilityPack.HtmlDocument();
                    doc.Load(result, Encoding.GetEncoding("ISO-8859-1"));
                    var tables = doc.DocumentNode.Descendants("table").ToList();

                    var table =
                        tables.First(
                            x => x.Attributes.Any(y => y.Name == "class" && y.Value == "table table-condensed leaderboard"));
                    var climbers = table.Descendants("tr").Skip(1).Select(x =>
                    {
                        var columns = x.Descendants("td").ToList();
                        var kongsveienPointsColumn = columns[2].Descendants("b").FirstOrDefault();
                        var kongsveienTimeColumn = columns[2].Descendants("a").FirstOrDefault();
                        var grefsenPointsColumn = columns[3].Descendants("b").FirstOrDefault();
                        var grefsenTimeColumn = columns[3].Descendants("a").FirstOrDefault();
                        var tryvannPointsColumn = columns[4].Descendants("b").FirstOrDefault();
                        var tryvannTimeColumn = columns[4].Descendants("a").FirstOrDefault();
                        return new
                        {
                            Place = columns[0].InnerText,
                            Name = columns[1].Descendants("a").First().InnerText,
                            KongsveienPoints = kongsveienPointsColumn != null ? kongsveienPointsColumn.InnerText : string.Empty,
                            KongsveienTime = kongsveienTimeColumn != null ? kongsveienTimeColumn.InnerText : string.Empty,
                            TryvannPoints = tryvannPointsColumn != null ? tryvannPointsColumn.InnerText : string.Empty,
                            TryvannTime = tryvannTimeColumn != null ? tryvannTimeColumn.InnerText : string.Empty,
                            GrefsenPoints = grefsenPointsColumn != null ? grefsenPointsColumn.InnerText : string.Empty,
                            GrefsenTime = grefsenTimeColumn != null ? grefsenTimeColumn.InnerText : string.Empty,
                            TotalPoints = columns[5].InnerText
                        };
                    }).ToList();

                    var ciberClimbers = climbers.Where(x => ciberNames.Contains(x.Name)).ToList();
                    foreach (var rider in ciberClimbers)
                    {
                        var cmodel = new ClimberModels
                        {
                            Name = rider.Name,
                            Place = rider.Place,
                            KongsveienPoints = rider.KongsveienPoints,
                            KongsveienTime = rider.KongsveienTime,
                            TryvannPoints = rider.TryvannPoints,
                            TryvannTime = rider.TryvannTime,
                            GrefsenPoint = rider.GrefsenPoints,
                            GrefsenTime = rider.GrefsenTime,
                            TotalPoints = rider.TotalPoints
                        };
                        climberList.Add(cmodel);
                    }
                }
                catch (Exception e)
                {
                    //var message = string.Format("Failed downloading stockchange info{0}{1}", Environment.NewLine, e);
                    //Trace.TraceError(message);
                    //_emailService.Send("perkristianhelland@gmail.com", "HPMC Nordnet fetch failed", message);
                    //return Enumerable.Empty<StockChange>();
                }

                //data = client.DownloadString(url);
            }

            return climberList;
        } 



        //// GET api/values
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/values/5
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/values
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/values/5
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //public void Delete(int id)
        //{
        //}
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
