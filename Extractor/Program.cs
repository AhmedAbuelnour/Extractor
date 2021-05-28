using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;

namespace Extractor
{
    class ThesisDocument
    {
        public string Title { get; set; }
        public string Published { get; set; }
        public string Publisher { get; set; }
        public List<Auther> Authers { get; set; } = new List<Auther>();
        public List<Header> Headers { get; set; } = new List<Header>();
        public List<string> Keywords { get; set; } = new List<string>();
    }
    class Auther
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Institution { get; set; }
        public string Country { get; set; }
    }

    class Header
    {
        public string Title { get; set; }
        public List<string> Content { get; set; }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            List<ThesisDocument> documents = new List<ThesisDocument>();

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object

            Console.WriteLine("Pass The TEI XML Directory");

            string DirectoryPath = Console.ReadLine();

            foreach (var documentPath in Directory.GetFiles(DirectoryPath, "*.xml"))
            {
                ThesisDocument document = new ThesisDocument();
                xmlDoc.Load(documentPath); // Load the XML document from the specified file
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
                        if (document.Authers.Count < 3)
                            document.Authers.Add(auther);
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
                            document.Title = child.InnerText;
                            break;
                        }
                    }
                }
                #endregion
                #region Extract Keywords
                XmlNodeList keywords = xmlDoc.GetElementsByTagName("keywords");
                foreach (XmlNode keyword in keywords)
                {
                    foreach (XmlNode term in keyword.ChildNodes)
                    {
                        if (!string.IsNullOrWhiteSpace(term.InnerText))
                            document.Keywords.Add(term.InnerText);
                    }
                }
                #endregion
                #region Headers & Content
                XmlNodeList Divs = xmlDoc.GetElementsByTagName("div");
                foreach (XmlNode item in Divs)
                {
                    if (item.ParentNode.Name.Equals("abstract")) // Abstract
                    {
                        document.Headers.Add(new Header
                        {
                            Title = "Abstract",
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
                            }
                            else if (childItem.Name.Equals("p"))
                            {
                                header.Content.Add(childItem.InnerText);
                            }
                        }
                        if (!string.IsNullOrEmpty(header.Title) && header.Content.Count > 0)
                            document.Headers.Add(header);
                    }
                }
                #endregion

                documents.Add(document);
            }
            await File.WriteAllTextAsync("Output.json", JsonSerializer.Serialize(documents));
        }
    }
}
