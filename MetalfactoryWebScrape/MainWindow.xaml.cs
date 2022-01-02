using HtmlAgilityPack;
using System;
using System.Collections.Generic;
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
            reportText.Text += "\nGet data from URL.";
            var concerts = GetHtmlInfo("https://metalfactory.ch/events");

            foreach (var item in concerts)
            {
                reportText.Text = item.ToString();
            }
        }

        private static List<string> GetHtmlInfo(string url)
        {
            var htmlInfo = new List<string>();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            HtmlNodeCollection linkNodes = doc.DocumentNode.SelectNodes("//h2/a");
            var baseUri = new Uri(url);

            foreach (HtmlNode node in linkNodes)
            {
                string href = node.Attributes["href"].Value;
                htmlInfo.Add(new Uri(baseUri, href).AbsoluteUri);
            }

            return htmlInfo;
        }
    }
}
