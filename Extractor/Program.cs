using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Extractor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<ThesisDocument> documents = new List<ThesisDocument>();
            Console.WriteLine("Pass The pdf thesis documents Directory");
            string DirectoryPath = Console.ReadLine();
            foreach (var documentPath in Directory.GetFiles(DirectoryPath, "*.pdf"))
            {
                ThesisDocument document = new ThesisDocument()
                {
                    PDFFilePath = documentPath
                };
                await document.GetHeaderAsync();
                await document.GetContentAsync();
                documents.Add(document);
            }
            Console.WriteLine($"Number of parsed doucments:{documents.Count}");
            await File.WriteAllTextAsync("Output.json", JsonSerializer.Serialize(documents));
        }
    }
}