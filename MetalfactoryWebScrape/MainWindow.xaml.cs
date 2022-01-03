using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            List<string> ConcertLinks = GetConcertLinks("https://metalfactory.ch/events");
            Task<List<Concert>> Concerts = GetConcerts(ConcertLinks);
            _ = MakeCalendarFile(Concerts);
        }

        private HtmlDocument GetDocument(string url)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);
            return doc;
        }

        private List<string> GetConcertLinks(string url)
        {
            var htmlInfo = new List<string>();
            HtmlDocument doc = GetDocument(url);
            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//h2/a");
            var baseUri = new Uri(url);

            foreach (HtmlNode node in linkNodes)
            {
                string href = node.Attributes["href"].Value;
                htmlInfo.Add(new Uri(baseUri, href).AbsoluteUri);
            }

            return htmlInfo;
        }

        private async Task<List<Concert>> GetConcerts(List<string> detailURLs)
        {
            List<Concert> concerts = new List<Concert>();
            int counter = 1;

            try
            {
                foreach (var link in detailURLs)
                {
                    HtmlDocument doc = GetDocument(link);
                    var concertBands = "//h1";
                    var concertDate = "//span[contains(@class, \"ic-single-next\")]";
                    var concertTime = "//span[contains(@class, \"ic-single-starttime\")]";
                    var concertLocation = "//div[contains(@class, \"details ic-details\")]/p";
                    var concert = new Concert
                    {
                        Bands = doc.DocumentNode.SelectSingleNode(concertBands).InnerHtml.Replace("&amp;", "&").Trim(),
                        Date = doc.DocumentNode.SelectSingleNode(concertDate).InnerHtml.Replace("&nbsp;", " "),
                        Time = doc.DocumentNode.SelectSingleNode(concertTime).InnerHtml,
                        Location = doc.DocumentNode.SelectSingleNode(concertLocation).InnerHtml.Replace("&nbsp;", "").Trim().Remove(0, 35)
                    };
                    concerts.Add(concert);
                    reportText.Text += counter.ToString() + ": " + concert.Bands + "\n";
                    counter++;
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
            catch (Exception ex)
            {
                reportText.Text = "Error in GetConcerts: " + ex.Message.ToString();
            }

            return concerts;
        }

        private async Task MakeCalendarFile(Task<List<Concert>> concertList)
        {
            foreach (var item in await concertList)
            {
                
            }
        }
    }
}
