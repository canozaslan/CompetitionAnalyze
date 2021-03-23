using Business.Abstract;
using Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class CompetitionManager : ICompetitionService
    {
        public WebClient pageClient { get; set; }
        public WebClient productClient { get; set; }
        public WebClient marketClient { get; set; }
        public HtmlDocument document { get; set; }
        public HtmlDocument pageDocument { get; set; }
        public HtmlDocument marketDocument { get; set; }
        private Uri url;

        public CompetitionManager()
        {

        }

        public async Task<Competition> GetDetail(string link)
        {
            productClient = qualifyClient(productClient);
            document = loadDocument(document, link, productClient);

            Competition competition = new Competition();

            var header = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/div/div[1]/h1")[0];
            competition.ProductName = header.InnerHtml;

            if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/p/i") == null)
            {
                var priceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/div/div[3]/span[1]/span")[0];
                var companyNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/div/div[3]/span[4]")[0];

                competition.Companies.Add(getCompanyName(companyNode.InnerHtml));
                competition.Prices.Add(convertToPrice(priceNode.InnerHtml));
            }
            else
            {
                var numberNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/p/i")[0];
                int number = getProductNumber(numberNode.InnerHtml);

                if (number > 0)
                {
                    for (int i = 1; i <= number; i++)
                    {
                        HtmlNode priceNode = null;

                        if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]") != null)
                            priceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]/ul/li[{i}]/a/span[1]/span")[0];
                        else if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]")[0] != null)
                            priceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]/ul/li[{i}]/a/span[1]/span")[0];

                        var price = convertToPrice(priceNode.InnerHtml);

                        if (price.Equals(", TL"))
                        {
                            HtmlNode nullPriceNode = null;
                            if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]") != null)
                                nullPriceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]/ul/li[{i}]/a/span[1]/span[2]")[0];
                            else if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]")[0] != null)
                                nullPriceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]/ul/li[{i}]/a/span[1]/span[2]")[0];

                            price = convertToPrice(nullPriceNode.InnerHtml);
                        }

                        HtmlNode companyNode = null;
                        if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]") != null)
                            companyNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]/ul/li[{i}]/a/span[5]/span")[0];
                        else if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]")[0] != null)
                            companyNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]/ul/li[{i}]/a/span[5]/span")[0];

                        competition.Prices.Add(price);
                        competition.Companies.Add(getCompanyName(companyNode.InnerHtml));
                    }
                }
            }
            return competition;
        }
        public async Task<List<Competition>> GetProducts(string product)
        {
            List<Competition> competitions = new List<Competition>();
            List<string> links = new List<string>();

            var firstLink = "https://www.akakce.com/arama/?q=" + product;

            marketClient = qualifyClient(marketClient);
            marketDocument = loadDocument(marketDocument, firstLink, marketClient);

            var countNode = marketDocument.DocumentNode.SelectSingleNode("/html/body/div[2]/p[2]/b");
            var count = Convert.ToInt32((countNode.InnerHtml).Substring((countNode.InnerHtml).IndexOf("/") + 1).Trim()) - 1;

            for (int k = 1; k < 3; k++)
            {
                var link = "https://www.akakce.com/arama/?q=" + product + $"&p={k}";

                pageClient = qualifyClient(pageClient);
                pageDocument = loadDocument(pageDocument, link, pageClient);

                for (int j = 1; j < 24; j++)
                {
                    var productLinks = pageDocument.DocumentNode.SelectSingleNode($"/html/body/div[2]/ul/li[{j}]//a");
                    if (productLinks == null) continue;
                    links.Add("https://www.akakce.com" + productLinks.Attributes["href"].Value);
                }

                foreach (var li in links)
                {
                    Competition competition = new Competition();

                    productClient = qualifyClient(productClient);
                    document = loadDocument(document, li, productClient);

                    var header = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/div/div[1]/h1")[0];
                    competition.ProductName = header.InnerHtml;

                    if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/p/i") == null)
                    {
                        var priceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/div/div[3]/span[1]/span")[0];
                        var companyNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/div/div[3]/span[4]")[0];

                        competition.Companies.Add(getCompanyName(companyNode.InnerHtml));
                        competition.Prices.Add(convertToPrice(priceNode.InnerHtml));

                    }
                    else
                    {
                        var numberNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/p/i")[0];
                        int number = getProductNumber(numberNode.InnerHtml);

                        if (number > 0)
                        {
                            for (int i = 1; i <= number; i++)
                            {
                                HtmlNode priceNode = null;
                                if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]") != null)
                                    priceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]/ul/li[{i}]/a/span[1]/span")[0];
                                else if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]")[0] != null)
                                    priceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]/ul/li[{i}]/a/span[1]/span")[0];

                                var price = convertToPrice(priceNode.InnerHtml);

                                if (price.Equals(", TL"))
                                {
                                    HtmlNode nullPriceNode = null;
                                    if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]") != null)
                                        nullPriceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]/ul/li[{i}]/a/span[1]/span[2]")[0];
                                    else if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]")[0] != null)
                                        nullPriceNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]/ul/li[{i}]/a/span[1]/span[2]")[0];

                                    price = convertToPrice(nullPriceNode.InnerHtml);
                                }

                                HtmlNode companyNode = null;
                                if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]") != null)
                                    companyNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[3]/ul/li[{i}]/a/span[5]/span")[0];
                                else if (document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]")[0] != null)
                                    companyNode = document.DocumentNode.SelectNodes($"/html/body/main/div[1]/section/div[2]/ul/li[{i}]/a/span[5]/span")[0];

                                competition.Prices.Add(price);
                                competition.Companies.Add(getCompanyName(companyNode.InnerHtml));
                            }
                        }
                    }
                    competitions.Add(competition);
                }
            }

            return competitions;
        }

        private int getProductNumber(string numb)
        {
            string a = string.Empty;
            int number;
            for (int i = 0; i < numb.Length; i++)
            {
                if (char.IsDigit(numb[i]))
                {
                    a += numb[i];
                }
            }
            number = Convert.ToInt32(a);
            if (number > 10) number = 10;
            return number;
        }

        private HtmlDocument loadDocument(HtmlDocument document, string link, WebClient client)
        {
            document = new HtmlDocument();

            url = new Uri(link);
            try
            {
                string html = client.DownloadString(url);
                document.LoadHtml(html);
            }
            catch (Exception e)
            {
                throw e;
            }
            return document;
        }
        private WebClient qualifyClient(WebClient client)
        {
            client = new WebClient();
            client.Encoding = Encoding.UTF8;
            client.Headers.Add("accept: text/html, application/xhtml+xml, */*");
            client.Headers.Add("user-agent: mozilla/5.0 (compatible; msie 9.0; windows nt 6.1; wow64; trident/5.0)");
            return client;
        }
        private string convertToPrice(string s)
        {
            string p = string.Empty;
            var n = 0;
            for (int j = 0; j < s.Length; j++)
            {


                if (char.IsDigit(s[j]))
                {

                    p += s[j];
                }
                else
                {
                    if (n == 0) p += ",";

                    if (j == s.Length - 1) p += " TL";
                    n++;
                }
            }
            return p;
        }
        private string getCompanyName(string st)
        {
            string company;
            if (st.Contains("<img"))
            {
                var q = st.Length;
                var stIndex = st.IndexOf("<span>") + 6;
                var fnIndex = st.Substring(stIndex).IndexOf("</span>");
                company = st.Substring(stIndex, fnIndex);
                var lastIndex = st.LastIndexOf("</span>") + 7;
                company += st.Substring(lastIndex);
            }
            else if (!st.Contains("<img") && st.Contains("<span"))
            {
                var stIndex = st.IndexOf("<span");
                company = st.Substring(0, stIndex);
            }
            else
            {
                company = st;
            }
            return company;
        }
    }
}
