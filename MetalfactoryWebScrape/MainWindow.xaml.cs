using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
            List<Concert> ConcertsList = ScrappedList.GroupBy(x => new { x.Bands, x.DateTimeStart }).Select(x => x.First()).ToList();
            StringBuilder AllConcerts = CreateEventFile(ConcertsList);
            SaveConcertsToFile(AllConcerts);
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
                string concertBands = detail.FirstChild.InnerText.Replace("Konzerte", "").Trim();
                string concertDate = "";
                if (detail.ChildNodes[2].InnerText.Contains("-"))
                {
                    concertDate = detail.ChildNodes[2].InnerText.Split('-')[0].Replace("&nbsp;", " ").Trim();
                }
                else
                {
                    concertDate = detail.ChildNodes[2].InnerText.Replace("&nbsp;", " ");
                }               
                string concertTime = concertDate.Substring(concertDate.Length - 6).Trim();
                string concertLocation = detail.ChildNodes[3].InnerText.Trim();
                CultureInfo deC = new CultureInfo("de-DE");
                string newDateString = concertDate.Remove(concertDate.Length - 6).Trim();
                DateTime newDate = DateTime.Parse(newDateString, deC, DateTimeStyles.NoCurrentDateDefault);
                DateTime newTime = DateTime.Parse(concertTime, CultureInfo.CurrentCulture);
                DateTime newDateTime = newDate + newTime.TimeOfDay;

                var concert = new Concert
                {
                    
                    Bands = concertBands.Replace("&amp;", "&"),
                    DateTimeStart = newDateTime,
                    Location = concertLocation
                };
                concerts.Add(concert);
                reportText.Text += counter.ToString() + ": " + concert.Bands + " *** Date: " + concert.DateTimeStart + " *** Location: " + concert.Location + "\n";
                counter++;
            }

            return concerts;
        }

        private string DateFormat
        {
            get { return "yyyyMMddTHHmmssZ"; }
        }

        private StringBuilder CreateEventFile(List<Concert> concertsList)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:WinterThane");
            sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("METHOD:PUBLISH");

            foreach (Concert concert in concertsList)
            {
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine("DTSTART:" + concert.DateTimeStart.AddHours(-1).ToString(DateFormat));
                sb.AppendLine("DTEND:" + concert.DateTimeStart.AddHours(2).ToString(DateFormat));
                sb.AppendLine("LOCATION:" + concert.Location);
                sb.AppendLine("DTSTAMP:" + DateTime.Now.ToString(DateFormat));
                sb.AppendLine("UID:" + DateTime.Now.ToString(DateFormat) + "WinterThane");
                sb.AppendLine("CREATED:" + DateTime.Now.ToString(DateFormat));
                sb.AppendLine("DESCRIPTION:" + concert.Bands);
                sb.AppendLine("LAST-MODIFIED:" + DateTime.Now.ToString(DateFormat));
                sb.AppendLine("SEQUENCE:0");
                sb.AppendLine("STATUS:CONFIRMED");
                sb.AppendLine("SUMMARY:" + concert.Bands);
                sb.AppendLine("TRANSP:OPAQUE");
                sb.AppendLine("END:VEVENT");
            }

            sb.AppendLine("END:VCALENDAR");

            return sb;
        }

        private void SaveConcertsToFile(StringBuilder allConcerts)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string destinationPath = Path.Combine(basePath, "EventFile");

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            string fullPath = destinationPath + "\\events.ics";
            File.WriteAllText(fullPath, allConcerts.ToString());
        }
    }
}
