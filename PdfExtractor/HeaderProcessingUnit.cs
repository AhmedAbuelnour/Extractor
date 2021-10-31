using RestSharp;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using ThesisModel;

namespace PdfExtractor
{
    public class HeaderProcessingUnit
    {
        private RestClient Client;
        private RestRequest Request;
        public string TEIContent { get; set; }
        private XmlDocument xmlDoc { get; set; }
        public HeaderProcessingUnit()
        {
            Client = new RestClient("http://localhost:8070/api/processHeaderDocument");
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

        public string GetPublishedDate()
        {
            XmlNodeList publiserStmt = xmlDoc.GetElementsByTagName("publicationStmt");
            foreach (XmlNode xmlNode in publiserStmt)
            {
                foreach (XmlNode publication in xmlNode.ChildNodes)
                {
                    if (publication.Name.Equals("date"))
                    {
                       return publication.InnerText;
                    }
                }
            }
            return string.Empty;
        }

        public ICollection<Author> GetAuthorsInfo()
        {
            XmlNodeList authorsNode = xmlDoc.GetElementsByTagName("author");
            List<Author> authors = new List<Author>();
            Author author = new Author();
            foreach (XmlNode AutherParent in authorsNode)
            {
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
                                    author.FirstName = NameXmlNode.InnerText;
                                }
                                if (NameXmlNode.Attributes["type"]?.Value.Equals("middle") ?? false)
                                {
                                    author.MiddleName = NameXmlNode.InnerText;
                                }
                            }
                            if (NameXmlNode.Name.Equals("surname"))
                            {
                                author.Surname = NameXmlNode.InnerText;
                            }
                        }
                    }
                    else if (AutherNodes.Name.Equals("email"))
                    {
                        author.Email = AutherNodes.InnerText;
                    }
                    else if (AutherNodes.Name.Equals("affiliation"))
                    {
                        foreach (XmlNode orgNode in AutherNodes.ChildNodes)
                        {
                            if (orgNode.Name.Equals("orgName") && string.IsNullOrEmpty(author.Department))
                            {
                                author.Department = orgNode.FirstChild.InnerText;
                                author.Institution = orgNode.LastChild.InnerText;
                            }
                            else if (orgNode.Name.Equals("address"))
                            {
                                foreach (XmlNode AddressNode in orgNode.ChildNodes)
                                {
                                    if (AddressNode.Name.Equals("country"))
                                    {
                                        author.Country = AddressNode.InnerText;
                                    }
                                }
                            }
                        }
                    }

                }
                if (!string.IsNullOrEmpty(author.FirstName) || !string.IsNullOrEmpty(author.MiddleName) || !string.IsNullOrEmpty(author.Surname))
                    authors.Add(author);
            }
            return authors;
        }

        public string GetThesisDocunetTitle()
        {
            XmlNodeList docTitle = xmlDoc.GetElementsByTagName("titleStmt");
            foreach (XmlNode docItem in docTitle)
            {
                foreach (XmlNode child in docItem.ChildNodes)
                {
                    if (child.Name.Equals("title"))
                    {
                        return child.InnerText;

                    }
                }
            }
            return string.Empty;
        }
    }
}
