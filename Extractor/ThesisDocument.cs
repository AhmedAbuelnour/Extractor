using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace Extractor
{
    public class ThesisDocument
    {
        [JsonIgnore]
        public string PDFFilePath { get; set; }
        public string Title { get; set; }
        public string Published { get; set; }
        public string Publisher { get; set; }
        public Dictionary<string, List<Header>> GroupedHeaders { get; set; }
        public List<Auther> Authers { get; set; } = new List<Auther>();
        private List<Header> Headers { get; set; } = new List<Header>();

        public async Task GetHeaderAsync()
        {
            RestClient client = new RestClient("http://localhost:8070/api/processHeaderDocument");
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.POST);
            request.AddFile("input", PDFFilePath);
            IRestResponse response = await client.ExecuteAsync(request);
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(response.Content);
            #region Auther
            XmlNodeList authorsNode = xmlDoc.GetElementsByTagName("author");
            foreach (XmlNode AutherParent in authorsNode)
            {
                Auther auther = new Auther();

                foreach (XmlNode AutherNodes in AutherParent.ChildNodes)
                {
                    if (AutherNodes.Name.Equals("persName"))
                    {
                        foreach (XmlNode NameXmlNode in AutherNodes.ChildNodes)
                        {
                            if (NameXmlNode.Name.Equals("forename"))
                            {
                                if (NameXmlNode.Attributes["type"]?.Value.Equals("first") ?? false)
                                {
                                    auther.FirstName = NameXmlNode.InnerText;
                                }
                                if (NameXmlNode.Attributes["type"]?.Value.Equals("middle") ?? false)
                                {
                                    auther.MiddleName = NameXmlNode.InnerText;
                                }
                            }
                            if (NameXmlNode.Name.Equals("surname"))
                            {
                                auther.Surname = NameXmlNode.InnerText;
                            }
                        }
                    }
                    else if (AutherNodes.Name.Equals("email"))
                    {
                        auther.Email = AutherNodes.InnerText;
                    }
                    else if (AutherNodes.Name.Equals("affiliation"))
                    {
                        foreach (XmlNode orgNode in AutherNodes.ChildNodes)
                        {
                            if (orgNode.Name.Equals("orgName") && string.IsNullOrEmpty(auther.Department))
                            {
                                auther.Department = orgNode.FirstChild.InnerText;
                                auther.Institution = orgNode.LastChild.InnerText;
                            }
                            else if (orgNode.Name.Equals("address"))
                            {
                                foreach (XmlNode AddressNode in orgNode.ChildNodes)
                                {
                                    if (AddressNode.Name.Equals("country"))
                                    {
                                        auther.Country = AddressNode.InnerText;
                                    }
                                }
                            }
                        }
                    }

                }

                if (!string.IsNullOrEmpty(auther.FirstName) || !string.IsNullOrEmpty(auther.MiddleName) || !string.IsNullOrEmpty(auther.Surname))
                    if (Authers.Count < 3)
                        Authers.Add(auther);
            }
            #endregion
            #region Thesis Name
            XmlNodeList docTitle = xmlDoc.GetElementsByTagName("titleStmt");
            foreach (XmlNode docItem in docTitle)
            {
                foreach (XmlNode child in docItem.ChildNodes)
                {
                    if (child.Name.Equals("title"))
                    {
                        Title = child.InnerText;
                        break;
                    }
                }
            }
            #endregion
        }

        public async Task GetContentAsync()
        {
            RestClient client = new RestClient("http://localhost:8070/api/processFulltextDocument");
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.POST);
            request.AddFile("input", PDFFilePath);
            IRestResponse response = await client.ExecuteAsync(request);
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.LoadXml(response.Content);
            #region Headers & Content
            XmlNodeList Divs = xmlDoc.GetElementsByTagName("div");
            foreach (XmlNode item in Divs)
            {
                if (item.ParentNode.Name.Equals("abstract")) // Abstract
                {
                    Headers.Add(new Header
                    {
                        Title = "Abstract",
                        Level = "Abstract",
                        Content = new List<string>() { item.InnerText }
                    });
                }
                else
                {
                    Header header = new Header
                    {
                        Content = new List<string>()
                    };
                    foreach (XmlNode childItem in item.ChildNodes)
                    {
                        if (childItem.Name.Equals("head"))
                        {
                            header.Title = childItem.InnerText;
                            header.Level = childItem.Attributes["n"]?.Value;
                        }
                        else if (childItem.Name.Equals("p"))
                        {
                            header.Content.Add(childItem.InnerText);
                        }
                    }
                    if (!string.IsNullOrEmpty(header.Title) && header.Content.Count > 0 && header.Level != null)
                        Headers.Add(header);
                }
            }
            #endregion
            GroupedHeaders = Headers.GroupBy(a => a.Level.Substring(0, 1)).ToDictionary(a => a.Key, a => a.ToList());
        }
    }

    public class Auther
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Institution { get; set; }
        public string Country { get; set; }
    }

    public class Header
    {
        public string Title { get; set; }
        public List<string> Content { get; set; }
        public string Level { get; set; }
    }
}
