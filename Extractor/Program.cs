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
            FullTextProcessingUnit fullTextProcessingUnit = new FullTextProcessingUnit();
            HeaderProcessingUnit headerProcessingUnit = new HeaderProcessingUnit();
            Console.WriteLine("Pass The pdf thesis documents Directory");

            string DirectoryPath = Console.ReadLine();
            foreach (var documentPath in Directory.GetFiles(DirectoryPath, "*.pdf"))
            {
                await fullTextProcessingUnit.SendFileAsync(documentPath);
                await headerProcessingUnit.SendFileAsync(documentPath);
                Thesis thesis = new Thesis
                {
                    Title = headerProcessingUnit.GetThesisDocunetTitle(),
                    PublishedDate = headerProcessingUnit.GetPublishedDate(),
                    Authors = headerProcessingUnit.GetAuthorsInfo(),
                    Abstract = fullTextProcessingUnit.GetAbstractInfo(),
                    Introduction = fullTextProcessingUnit.GetIntroductionInfo(),
                    FutureWork = fullTextProcessingUnit.GetFutureWorkInfo(),
                    Keywords = fullTextProcessingUnit.GetKeywords(),
                };
                thesisDocumentDbContext.Theses.Add(thesis);
            }
            thesisDocumentDbContext.SaveChanges();
        }
    }
}