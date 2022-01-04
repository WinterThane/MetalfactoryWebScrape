using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MetalfactoryWebScrape
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Make_File_Button_Click(object sender, RoutedEventArgs e)
        {
            List<Concert> ScrappedList = GetConcertLinks("https://metalfactory.ch/events");
            List<Concert> ConcertsList = ScrappedList.GroupBy(x => new { x.Bands, x.Date }).Select(x => x.First()).ToList();
            var xxx = "";
        }

        private HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }

        private List<Concert> GetConcertLinks(string url)
        {
            List<Concert> concerts = new List<Concert>();
            int counter = 1;
            HtmlDocument doc = GetDocument(url);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, \"ic-content\")]");

            foreach (var detail in linkNodes)
            {
                var concertBands = detail.FirstChild.InnerText.Replace("Konzerte", "").Trim();
                var concertDate = "";
                if (detail.ChildNodes[2].InnerText.Contains("-"))
                {
                    concertDate = detail.ChildNodes[2].InnerText.Split('-')[0].Replace("&nbsp;", " ").Trim();
                }
                else
                {
                    concertDate = detail.ChildNodes[2].InnerText.Replace("&nbsp;", " ");
                }               
                var concertTime = concertDate.Substring(concertDate.Length - 6).Trim();
                var concertLocation = detail.ChildNodes[3].InnerText.Trim();
                var concert = new Concert
                {
                    
                    Bands = concertBands.Replace("&amp;", "&"),
                    Date = concertDate.Remove(concertDate.Length - 6).Trim(),
                    Time = concertTime,
                    Location = concertLocation
                };
                concerts.Add(concert);
                reportText.Text += counter.ToString() + ": " + concert.Bands + " *** Date: " + concert.Date + " *** Time: " + concert.Time + " *** Location: " + concert.Location + "\n";
                counter++;
            }

            return concerts;
        }
    }
}
