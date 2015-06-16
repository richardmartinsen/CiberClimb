using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CiberClimb.Models;
using Newtonsoft.Json;

namespace CiberClimb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            const string url = "http://www.klatrekonge.com/herrer-oslo";

            var climberList = new List<ClimberModels>();
            using (var client = new HttpClient())
            {
                var ciberNames = new[] { "Mathias Moen", "Richard Martinsen", "Joakim Bjerkheim" };

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
                        var tryvannPointsColumn = columns[3].Descendants("b").FirstOrDefault();
                        var tryvannTimeColumn = columns[3].Descendants("a").FirstOrDefault();
                        var grefsenPointsColumn = columns[4].Descendants("b").FirstOrDefault();
                        var grefsenTimeColumn = columns[4].Descendants("a").FirstOrDefault();
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

            //string urlWithAccessToken = "https://cibernordic.slack.com/services/hooks/incoming-webhook?token={your_access_token}";
            string urlWithAccessToken = "https://hooks.slack.com/services/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";
	
	        SlackClient client = new SlackClient(urlWithAccessToken);
	
	        client.PostMessage(username: "Mr. Torgue",
			                       text: "THIS IS A TEST MESSAGE! SQUEEDLYBAMBLYFEEDLYMEEDLYMOWWWWWWWW!",
			                    channel: "#bot-test");

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