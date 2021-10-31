using PdfExtractor;
using System;
using System.IO;
using System.Threading.Tasks;
using ThesisModel;

namespace Extractor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ThesisDocumentDbContext thesisDocumentDbContext = new ThesisDocumentDbContext();
            thesisDocumentDbContext.Database.EnsureCreated();
            Console.WriteLine("Pass The pdf thesis documents Directory");
            string DirectoryPath = Console.ReadLine();
            Console.WriteLine("Load from \n 1-API \n 2-Locally");
            int option = int.Parse(Console.ReadLine());
            foreach (var documentPath in Directory.GetFiles(DirectoryPath))
            {
                FullTextProcessingUnit fullTextProcessingUnit = new FullTextProcessingUnit();
                HeaderProcessingUnit headerProcessingUnit = new HeaderProcessingUnit();
                if (option == 1)
                {
                    await fullTextProcessingUnit.SendFileAsync(documentPath);
                    await headerProcessingUnit.SendFileAsync(documentPath);
                }
                else
                {
                    fullTextProcessingUnit.LoadTEIFile(documentPath);
                    headerProcessingUnit.LoadTEIFile(documentPath);
                }
                Thesis thesis = new Thesis
                {
                    Title = headerProcessingUnit.GetThesisDocunetTitle() ?? Path.GetFileNameWithoutExtension(documentPath),
                    PublishedDate = headerProcessingUnit.GetPublishedDate(),
                    Authors = headerProcessingUnit.GetAuthorsInfo(),
                    Abstract = fullTextProcessingUnit.GetAbstractInfo(),
                    FutureWork = fullTextProcessingUnit.GetFutureWorkInfo(),
                    Keywords = fullTextProcessingUnit.GetKeywords(),
                };
                thesisDocumentDbContext.Theses.Add(thesis);
                await thesisDocumentDbContext.SaveChangesAsync();
            }
        }
    }
}