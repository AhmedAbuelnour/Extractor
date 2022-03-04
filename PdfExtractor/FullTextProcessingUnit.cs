using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
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
            await File.WriteAllTextAsync($"{Path.GetDirectoryName(pdfFilePath)}/{Path.GetFileNameWithoutExtension(pdfFilePath)}.tei.xml", TEIContent);
        }
        public void LoadTEIFile(string filePath)
        {
            xmlDoc.Load(filePath);
        }
        //public IEnumerable<Keyword> GetKeywords()
        //{
        //    foreach (XmlNode keyword in xmlDoc.GetElementsByTagName("keywords"))
        //    {
        //        foreach (XmlNode term in keyword.ChildNodes)
        //        {
        //            if (!string.IsNullOrWhiteSpace(term.InnerText))
        //                yield return new Keyword
        //                {
        //                    Term = term.InnerText
        //                };
        //        }
        //    }
        //}
        public string GetAbstractInfo()
        {
            // case 1
            XmlNodeList abstracts = xmlDoc.GetElementsByTagName("teiHeader");
            foreach (XmlNode item in abstracts)
            {
                foreach (var child in item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("profileDesc")))
                {
                    string extractedAbstract = child.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("abstract")).Select(a => a.InnerText).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(extractedAbstract))
                        return extractedAbstract;
                }

            }
            // case 2
            XmlNodeList Divs = xmlDoc.GetElementsByTagName("div");
            foreach (XmlNode item in Divs.Cast<XmlNode>().Where(a => a.Name.Equals("head")))
            {
                return item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("Abstract", StringComparison.OrdinalIgnoreCase)).FirstOrDefault().InnerText;
            }
            // case 3 , fall back to introduction
            foreach (XmlNode item in Divs)
            {
                // case 1, header equals Future work
                foreach (XmlNode childItem in item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("head")))
                {
                    if (childItem.InnerText.Trim().StartsWith("Introduction", StringComparison.OrdinalIgnoreCase))
                        return string.Join(' ', item.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("p")).Select(a => a.InnerText));
                }
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
                return docItem.ChildNodes.Cast<XmlNode>().Where(a => a.Name.Equals("title")).Select(a => a.InnerText).FirstOrDefault();
            return String.Empty;
        }
    }
}
