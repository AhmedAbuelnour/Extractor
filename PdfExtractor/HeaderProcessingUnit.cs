using RestSharp;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public PdfLoadedDocument PDFMetadata { get; set; }
        public HeaderProcessingUnit()
        {
            Client = new RestClient("http://localhost:8070/api/processHeaderDocument");
            Client.Timeout = -1;
            Request = new RestRequest(Method.POST);
            xmlDoc = new XmlDocument();
        }
        public async Task SendFileAsync(string pdfFilePath)
        {
            PDFMetadata = new PdfLoadedDocument(File.ReadAllBytes((pdfFilePath)));
            Request.AddFile("input", pdfFilePath);
            IRestResponse response = await Client.ExecuteAsync(Request);
            TEIContent = response.Content;
            xmlDoc.LoadXml(TEIContent);
        }

        public void LoadTEIFile(string filePath)
        {
            xmlDoc.Load(filePath);
            

        }

    }
}
