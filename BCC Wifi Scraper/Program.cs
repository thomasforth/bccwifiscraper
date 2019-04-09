using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BCC_Wifi_Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            List<WifiPoint> WifiPoints = new List<WifiPoint>();

            string baseWebsite = @"https://www.birmingham.gov.uk/directory/44/wifi_zones_in_birmingham/category/839";

            HtmlWeb htmlWeb = new HtmlWeb();
            htmlWeb.OverrideEncoding = Encoding.UTF8;
            HtmlDocument htmlDocument = htmlWeb.Load(baseWebsite);

            // from https://stackoverflow.com/questions/13225438/htmlagilitypack-selectnodes-expression-to-ignore-an-element-with-a-certain-attri
            if (htmlDocument.DocumentNode.SelectNodes("//*[contains(@class, 'list__link')]") != null)
            {
                foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//*[contains(@class, 'list__link')]"))
                {
                    WifiPoint wifiPoint = new WifiPoint();

                    wifiPoint.name = node.InnerText.Trim() + " ";
                    wifiPoint.URL = @"https://www.birmingham.gov.uk/" + node.GetAttributeValue("href", string.Empty);

                    HtmlWeb htmlWeb2 = new HtmlWeb();
                    htmlWeb2.OverrideEncoding = Encoding.UTF8;
                    HtmlDocument htmlDocument2 = htmlWeb2.Load(wifiPoint.URL);
                    wifiPoint.URL = wifiPoint.URL.Replace("//d", "/d");
                    if (htmlDocument2.DocumentNode.SelectNodes("//*[contains(@id, 'map_marker_location_400')]") != null)
                    {
                        foreach (HtmlNode node2 in htmlDocument2.DocumentNode.SelectNodes("//*[contains(@id, 'map_marker_location_400')]"))
                        {
                            string longlatblock = node2.GetAttributeValue("value", string.Empty);
                            if (longlatblock != null)
                            {
                                wifiPoint.longitude = longlatblock.Split(",")[0].Substring(0,8);
                                wifiPoint.latitude = longlatblock.Split(",")[1].Substring(0, 8);
                            }
                        }
                    }

                    WifiPoints.Add(wifiPoint);
                }
            }

            // write it out
            using (TextWriter writer = new StreamWriter(@"WiFiPoints.csv", false, System.Text.Encoding.UTF8))
            {
                var csv = new CsvWriter(writer);
                csv.WriteRecords(WifiPoints.Where(x => x.longitude != null));
            }
        }
    }

    public class WifiPoint
    {
        public string name { get; set; }
        public string URL { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }
    }
}
