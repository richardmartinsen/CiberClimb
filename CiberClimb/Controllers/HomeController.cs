using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CiberClimb.Models;

namespace CiberClimb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string data = "";
            string url = "http://www.klatrekonge.com/herrer-oslo";

            var climberList = new List<ClimberModels>();
            using (HttpClient client = new HttpClient())
            {
                var time = DateTime.UtcNow;
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
                    StringBuilder sb = new StringBuilder();
                    //data = climbers.ToString();
                    foreach (var rider in ciberClimbers)
                    {

                        var cmodel = new ClimberModels();
                        cmodel.Name = rider.Name;
                        cmodel.Place = rider.Place;
                        cmodel.KongsveienPoints = rider.KongsveienPoints;
                        cmodel.KongsveienTime = rider.KongsveienTime;
                        cmodel.TryvannPoints = rider.TryvannPoints;
                        cmodel.TryvannTime = rider.TryvannTime;
                        cmodel.GrefsenPoint = rider.GrefsenPoints;
                        cmodel.GrefsenTime = rider.GrefsenTime;
                        cmodel.TotalPoints = rider.TotalPoints;
                        climberList.Add(cmodel);
                        //sb.AppendLine(rider.Name);
                    }
                    //data = sb.ToString();
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

            return View();
        }
    }
}