using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CiberClimb.Models;

namespace CiberClimb.Services
{
    public class ClimberService
    {

        public async Task<List<ClimberModels>> GetClimberModels()
        {
            var climberList = new List<ClimberModels>();
            using (var client = new HttpClient())
            {
                var ciberNames = new[] { "Mathias Moen", "Richard Martinsen", "Joakim Bjerkheim", 
                                         "Kyrre Havik Eriksen", "Morten Midttun", "Kjetil Kronkvist", 
                                         "Aleks Gisvold", "Magnus Moltzau", "Torstein Jensen", 
                                         "Njaal Gjerde", "Håvard Vegge", "Jørgen Bugge", "Geir Sande", "Rafa S", "Terje Rabben"};

                try
                {
                    var urls = Enumerable.Range(0, 11).Select(x => "http://www.klatrekonge.com/herrer-oslo?page=" + x);
                    var resultTasks = urls.Select(url => client.GetStreamAsync(url)).ToList();
                    await Task.WhenAll(resultTasks);

                    foreach (var result in resultTasks)
                    {
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.Load(result.Result, Encoding.GetEncoding("utf-8"));
                        var tables = doc.DocumentNode.Descendants("table").ToList();

                        var table =
                            tables.First(
                                x =>
                                x.Attributes.Any(
                                    y => y.Name == "class" && y.Value == "table table-condensed leaderboard"));
                        var climbers = table.Descendants("tr").Skip(1).Select(
                            x =>
                            {
                                var columns = x.Descendants("td").ToList();
                                var kongsveienPointsColumn = columns[2].Descendants("b").FirstOrDefault();
                                var kongsveienTimeColumn = columns[2].Descendants("a").FirstOrDefault();
                                var grefsenPointsColumn = columns[3].Descendants("b").FirstOrDefault();
                                var grefsenTimeColumn = columns[3].Descendants("a").FirstOrDefault();
                                var tryvannPointsColumn = columns[4].Descendants("b").FirstOrDefault();
                                var tryvannTimeColumn = columns[4].Descendants("a").FirstOrDefault();
                                return
                                    new
                                    {
                                        Place = columns[0].InnerText,
                                        Name = columns[1].Descendants("a").First().InnerText,
                                        KongsveienPoints =
                                            kongsveienPointsColumn != null
                                                ? kongsveienPointsColumn.InnerText
                                                : string.Empty,
                                        KongsveienTime =
                                            kongsveienTimeColumn != null
                                                ? kongsveienTimeColumn.InnerText
                                                : string.Empty,
                                        TryvannPoints =
                                            tryvannPointsColumn != null
                                                ? tryvannPointsColumn.InnerText
                                                : string.Empty,
                                        TryvannTime =
                                            tryvannTimeColumn != null ? tryvannTimeColumn.InnerText : string.Empty,
                                        GrefsenPoints =
                                            grefsenPointsColumn != null
                                                ? grefsenPointsColumn.InnerText
                                                : string.Empty,
                                        GrefsenTime =
                                            grefsenTimeColumn != null ? grefsenTimeColumn.InnerText : string.Empty,
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
    }
}
