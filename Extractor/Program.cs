using Embedder;
using Microsoft.EntityFrameworkCore;
using PdfExtractor;
using PretrainedModelPicker;
using Summarizer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThesisModel;

namespace Extractor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string pythonExe = @"D:\MASTER WORK\After Proposal\Text Summarization Code\venv\Scripts\python.exe";
            string pythonFutureSummarizerPath = @"D:\MASTER WORK\After Proposal\Text Summarization Code\summy.py";
            string pythonModelPickerPath = @"D:\MASTER WORK\After Proposal\Text Summarization Code\picker.py";
            string pythonSBertPath = @"D:\MASTER WORK\After Proposal\Text Summarization Code\embedder.py";
            string pythonSimilaritytPath = @"D:\MASTER WORK\After Proposal\Text Summarization Code\embeding_similarity.py";

            Console.WriteLine("Do you want to perform \n 1- Search  \n 2- Pre-processing");

            int performPick = int.Parse(Console.ReadLine());

            if (performPick == 1)
            {
                await Search(pythonExe, pythonSBertPath, pythonSimilaritytPath);
            }
            else
            {
                await PreProcessing(pythonExe, pythonFutureSummarizerPath, pythonModelPickerPath, pythonSBertPath);
            }
        }
        static async Task Search(string pythonPath, string embedderPath, string embeddingSimilarityPath)
        {
            Console.WriteLine("Enter your search keywords");
            string keywords = Console.ReadLine().Trim();

            Console.WriteLine("Pass the directoy which will contain the temp embeddings for the keywords");
            string embeddingDirctory = Console.ReadLine().Trim();

            Console.WriteLine("Obtaining Embeddings for your keywords... (loading)");
            string keywordsEmbeddings = await new SEmbedder(pythonPath, embedderPath).GetEmbeddingPathAsync(keywords, embeddingDirctory, Path.GetRandomFileName());
            Console.WriteLine("Obtaining Embeddings for your keywords... (done)");

            Console.WriteLine("Enter top N selection");
            int topN = int.Parse(Console.ReadLine());

            Console.WriteLine($"Getting Top {topN} result");


            ThesisDocumentDbContext thesisDocumentDbContext = new ThesisDocumentDbContext();


            List<EmbeddingResult> embeddingResults = new List<EmbeddingResult>();

            foreach (var thesis in thesisDocumentDbContext.Theses.ToList())
            {
                EmbeddingSimilarityCalculator embeddingSimilarityCalculator = new EmbeddingSimilarityCalculator(pythonPath, embeddingSimilarityPath);

                embeddingResults.Add(new EmbeddingResult
                {
                    ThesisId = thesis.Id,
                    Score = await embeddingSimilarityCalculator.GetEmbeddingScoreAsync(keywordsEmbeddings.Trim(), thesis.EmbeddingPath.Trim()),
                });
            }

            List<EmbeddingResult> topNembeddingResults = embeddingResults.OrderByDescending(a => a.Score).Take(topN).ToList();

            foreach (var item in topNembeddingResults)
            {
                Console.WriteLine("----------------------------------------------");
                Console.WriteLine($"Thesis Id: {item.ThesisId} || " + thesisDocumentDbContext.Theses.Where(a => a.Id == item.ThesisId).Include(a => a.SummarizedThesis).SingleOrDefault().SummarizedThesis.SummarizedFuturWork);
                Console.WriteLine("----------------------------------------------");
            }

            while (true)
            {
                Console.WriteLine("Which futur work you would like to get its related thesis documents?");

                int selectedThesisDocment = int.Parse(Console.ReadLine());

                string embeddingOfSelectedThesisDocument = thesisDocumentDbContext.Theses.SingleOrDefault(a => a.Id == selectedThesisDocment).EmbeddingPath;

                List<EmbeddingResult> subEmbeddingResults = new List<EmbeddingResult>();

                foreach (var thesis in thesisDocumentDbContext.Theses.ToList())
                {
                    EmbeddingSimilarityCalculator embeddingSimilarityCalculator = new EmbeddingSimilarityCalculator(pythonPath, embeddingSimilarityPath);

                    subEmbeddingResults.Add(new EmbeddingResult
                    {
                        ThesisId = thesis.Id,
                        Score = await embeddingSimilarityCalculator.GetEmbeddingScoreAsync(embeddingOfSelectedThesisDocument.Trim(), thesis.EmbeddingPath.Trim()),
                    });
                }

                List<EmbeddingResult> topsubeEmbeddingResults = subEmbeddingResults.OrderByDescending(a => a.Score).Take(topN).ToList();

                foreach (var item in topsubeEmbeddingResults)
                {
                    Console.WriteLine("----------------------------------------------");
                    Console.WriteLine($"Thesis Id: {item.ThesisId} || " + thesisDocumentDbContext.Theses.Where(a => a.Id == item.ThesisId).Include(a => a.SummarizedThesis).SingleOrDefault().SummarizedThesis.SummarizedFuturWork);
                    Console.WriteLine("----------------------------------------------");
                }

                Console.WriteLine("Do you want to exit? (y/n)");
                char yesOrNo = char.Parse(Console.ReadLine());
                if (yesOrNo == 'y' || yesOrNo == 'Y') break;
            }

        }
        static async Task PreProcessing(string pythonPath, string futureSummarizerPath, string modelPickerPath, string embedderPath)
        {
            int Correct = 0;
            int Failed = 0;

            ThesisDocumentDbContext thesisDocumentDbContext = new ThesisDocumentDbContext();

            thesisDocumentDbContext.Database.EnsureCreated();
            Console.WriteLine("Pass The pdf thesis documents Directory");
            string DirectoryPath = Console.ReadLine();

            Console.WriteLine("Pass the directoy which will contain the embeddings");
            string embeddingDirctory = Console.ReadLine();

            Console.WriteLine("Load from \n 1-Only have PDFs and want to process them online \n 2-Having TEI files Locally");
            int option = int.Parse(Console.ReadLine());

            foreach (var documentPath in Directory.GetFiles(DirectoryPath))
            {
                FullTextProcessingUnit fullTextProcessingUnit = new FullTextProcessingUnit();
                FutueWorkSummarizer futueWorkSummarizer = new FutueWorkSummarizer(pythonPath, futureSummarizerPath);
                ModelPicker modelPicker = new ModelPicker(pythonPath, modelPickerPath);
                SEmbedder embedder = new SEmbedder(pythonPath, embedderPath);
                if (option == 1)
                {
                    try
                    {
                        await fullTextProcessingUnit.SendFileAsync(documentPath);
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        fullTextProcessingUnit.LoadTEIFile(documentPath);
                    }
                    catch
                    {
                        continue;
                    }
                }
                Thesis thesis = new Thesis
                {

                    Title = Path.GetFileNameWithoutExtension(documentPath),
                    FutureWork = fullTextProcessingUnit.GetFutureWorkInfo(),
                    PublishedDate = fullTextProcessingUnit.GetPublishedDate(),
                    Abstract = fullTextProcessingUnit.GetAbstractInfo().Trim(),

                };
                if (thesis.Abstract == string.Empty || thesis.FutureWork == string.Empty || thesis.FutureWork.Length < 25)
                {
                    Failed++;
                    Console.WriteLine(thesis.Abstract == String.Empty ? $"Abstract is empty: {thesis.Title}" : $"future work is empty: {thesis.Title}");
                    continue;
                }
                try
                {
                    thesis.EmbeddingPath = (await embedder.GetEmbeddingPathAsync(thesis.Abstract, embeddingDirctory, thesis.Title)).Trim();
                    SummarizationResult summarizationResult = await futueWorkSummarizer.GetSummarizationResultAsync(thesis.FutureWork);
                    if (string.IsNullOrWhiteSpace(summarizationResult.SCIBert) || string.IsNullOrWhiteSpace(summarizationResult.RoBERTa))
                    {
                        Failed++;
                        Console.WriteLine("summarization is empty");
                        continue;
                    }
                    PickerResult pickerResult = await modelPicker.GetModelAsync(thesis.Abstract, summarizationResult.SCIBert, summarizationResult.RoBERTa);
                    if (string.IsNullOrWhiteSpace(pickerResult.Model) || string.IsNullOrWhiteSpace(pickerResult.Keywords))
                    {
                        Failed++;
                        Console.WriteLine("model picker is empty");
                        continue;
                    }
                    thesis.SummarizedThesis = new SummarizedThesis
                    {
                        ModelName = pickerResult.Model,
                        SummarizedFuturWork = pickerResult.Model == "RoBERTa" ? summarizationResult.RoBERTa : summarizationResult.SCIBert,
                        Keywords = pickerResult.Keywords,
                    };
                    thesisDocumentDbContext.Theses.Add(thesis);
                    await thesisDocumentDbContext.SaveChangesAsync();
                    Correct++;
                    Console.WriteLine(Correct);
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            Console.WriteLine($"Total Failed = {Failed}");
            Console.WriteLine($"Total Correct = {Correct}");
            Console.WriteLine($"Total Thesis = {Failed + Correct}");
        }
    }
}