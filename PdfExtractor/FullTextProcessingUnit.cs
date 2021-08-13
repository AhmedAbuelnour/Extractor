using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using ThesisModel;

namespace PdfExtractor
{
    public class FullTextProcessingUnit
    {
        private RestClient Client;
        private RestRequest Request;
        public string TEIContent { get; set; }
        private XmlDocument xmlDoc { get; set; }
        public FullTextProcessingUnit()
        {
            Client = new RestClient("http://localhost:8070/api/processFulltextDocument");
            Client.Timeout = -1;
            Request = new RestRequest(Method.POST);
            xmlDoc = new XmlDocument();
        }
        public async Task SendFileAsync(string pdfFilePath)
        {
            Request.AddFile("input", pdfFilePath);
            IRestResponse response = await Client.ExecuteAsync(Request);
            TEIContent = response.Content;
            xmlDoc.LoadXml(TEIContent);
        }
        public ICollection<Keyword> GetKeywords()
        {
            List<Keyword> Keywordlist = new List<Keyword>();
            XmlNodeList keywords = xmlDoc.GetElementsByTagName("keywords");
            foreach (XmlNode keyword in keywords)
            {
                foreach (XmlNode term in keyword.ChildNodes)
                {
                    if (!string.IsNullOrWhiteSpace(term.InnerText))
                        Keywordlist.Add(new Keyword
                        {
                            Term = term.InnerText
                        });
                }
            }
            return Keywordlist;
        }
        public string GetAbstractInfo()
        {
            XmlNodeList Divs = xmlDoc.GetElementsByTagName("div");
            foreach (XmlNode item in Divs)
            {
                if (item.ParentNode.Name.Equals("abstract")) // Abstract
                {
                    return item.InnerText;
                }
            }
            return string.Empty;
        }
        public string GetIntroductionInfo()
        {
            XmlNodeList Divs = xmlDoc.GetElementsByTagName("div");
            foreach (XmlNode item in Divs)
            {
                foreach (XmlNode childItem in item.ChildNodes)
                {
                    if (childItem.Name.Equals("head"))
                    {
                        if (childItem.InnerText.Contains("Introduction", StringComparison.OrdinalIgnoreCase) || childItem.InnerText.Contains("Background", StringComparison.OrdinalIgnoreCase) || childItem.InnerText.Contains("Overview", StringComparison.OrdinalIgnoreCase))
                        {
                            return string.Join(' ',item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("p")).Select(a => a.InnerText));
                        }
                    }
                }
            }
            return string.Empty;
        }
        public string GetFutureWorkInfo()
        {
            XmlNodeList Divs = xmlDoc.GetElementsByTagName("div");
            foreach (XmlNode item in Divs)
            {
                foreach (XmlNode childItem in item.ChildNodes)
                {
                    if (childItem.Name.Equals("head"))
                    {
                        if (childItem.InnerText.Contains("Future", StringComparison.OrdinalIgnoreCase) || childItem.InnerText.Contains("Summary", StringComparison.OrdinalIgnoreCase))
                        {
                            return string.Join(' ', item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("p")).Select(a => a.InnerText));
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
