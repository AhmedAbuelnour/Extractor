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
        public void LoadTEIFile(string filePath)
        {
            xmlDoc.Load(filePath);
        }
        public IEnumerable<Keyword> GetKeywords()
        {
            foreach (XmlNode keyword in xmlDoc.GetElementsByTagName("keywords"))
            {
                foreach (XmlNode term in keyword.ChildNodes)
                {
                    if (!string.IsNullOrWhiteSpace(term.InnerText))
                        yield return new Keyword
                        {
                            Term = term.InnerText
                        };
                }
            }
        }
        public string GetAbstractInfo()
        {
            // case 1
            XmlNodeList abstracts = xmlDoc.GetElementsByTagName("teiHeader");
            foreach (XmlNode item in abstracts.Cast<XmlNode>().Where(a => a.Name.Equals("profileDesc")))
            {
                if (item.ParentNode.Name.Equals("abstract")) // Abstract
                {
                    if (!string.IsNullOrWhiteSpace(item.InnerText))
                        return item.InnerText;
                }
            }
            // case 2
            XmlNodeList Divs = xmlDoc.GetElementsByTagName("div");
            foreach (XmlNode item in Divs.Cast<XmlNode>().Where(a => a.Name.Equals("head")))
            {
                return item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("Abstract")).FirstOrDefault().InnerText;
            }
            return string.Empty;
        }
        public string GetFutureWorkInfo()
        {
            XmlNodeList Divs = xmlDoc.GetElementsByTagName("div");

            foreach (XmlNode item in Divs)
            {
                // case 1, header equals Future work
                foreach (XmlNode childItem in item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("head")))
                {
                    if (childItem.InnerText.Trim().StartsWith("Future Work", StringComparison.OrdinalIgnoreCase))
                        return string.Join(' ', item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("p")).Select(a => a.InnerText));
                }
            }

            foreach (XmlNode item in Divs)
            {
                // case 2, header equals Conclusion and future work but must have inner text inside
                foreach (XmlNode childItem in item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("head")))
                {
                    if (childItem.InnerText.Trim().Contains("Conclusion and future work", StringComparison.OrdinalIgnoreCase))
                    {
                        var items = item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("p"));
                        if (items.Any())
                        {
                            return string.Join(' ', items.Select(a => a.InnerText));
                        }
                    }
                }
            }

            foreach (XmlNode item in Divs)
            {
                // case 3, header equals Conclusion but must have inner text inside
                foreach (XmlNode childItem in item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("head")))
                {
                    if (childItem.InnerText.Trim().StartsWith("Conclusion", true, null))
                    {
                        var items = item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("p"));
                        if (items.Count() > 0)
                        {
                            return string.Join(' ', items.Select(a => a.InnerText));
                        }
                    }
                }
            }
            return string.Empty;
        }
    }
}
