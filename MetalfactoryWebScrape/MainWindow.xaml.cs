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
            CreateeventFiles(ConcertsList);
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
                    
                    Bands = concertBands.Replace("&amp;", "_and_"),
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

        private void CreateeventFiles(List<Concert> concertsList)
        {
            

            foreach (Concert concert in concertsList)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("BEGIN:VCALENDAR");
                sb.AppendLine("PRODID:WinterThane");
                sb.AppendLine("VERSION:2.0");
                sb.AppendLine("BEGIN:VEVENT");
                sb.AppendLine("DESCRIPTION:" + concert.Bands);
                sb.AppendLine("DTEND:" + concert.DateTimeStart.AddHours(2).ToString(DateFormat));
                sb.AppendLine("DTSTAMP:" + DateTime.Now.ToString(DateFormat));
                sb.AppendLine("DTSTART:" + concert.DateTimeStart.AddHours(-1).ToString(DateFormat));
                sb.AppendLine("LOCATION:" + concert.Location);
                sb.AppendLine("SEQUENCE:0");
                sb.AppendLine("SUMMARY:" + concert.Bands);
                sb.AppendLine("UID:" + DateTime.Now.ToString(DateFormat) + "WinterThane");
                sb.AppendLine("BEGIN:VALARM");
                sb.AppendLine("ACTION:DISPLAY");
                sb.AppendLine("DESCRIPTION:REMINDER");
                sb.AppendLine("TRIGGER:-PT72H");
                sb.AppendLine("END:VALARM");
                sb.AppendLine("END:VEVENT");
                sb.AppendLine("END:VCALENDAR");

                string tmpName1 = concert.Bands.Replace(",", "_");
                string tmpName2 = tmpName1.Replace(":", "-");
                string newName;
                if (tmpName2.Length > 100)
                {
                    newName = tmpName2.Substring(0, 100);
                }
                else
                {
                    newName = tmpName2;
                }

                string tmpLoc = concert.Location.Replace(",", "_");

                SaveConcertsToFiles(sb, newName.Replace(" ", "_").Trim() + "___" + tmpLoc.Replace(" ", "_").Trim());
            }

            
        }

        private void SaveConcertsToFiles(StringBuilder allConcerts, string name)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string destinationPath = Path.Combine(basePath, "EventFile");

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            string fullPath = destinationPath + "\\" + name + ".ics";
            try
            {
                File.WriteAllText(fullPath, allConcerts.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "name: " + name);
            }
            
        }
    }
}
