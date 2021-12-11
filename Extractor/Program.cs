using PdfExtractor;
using PretrainedModelPicker;
using Summarizer;
using System;
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
            int Correct = 0;
            int Failed = 0;
            string pythonExe = @"D:\MASTER WORK\After Proposal\Text Summarization Code\venv\Scripts\python.exe";
            string pythonFutureSummarizerPath = @"D:\MASTER WORK\After Proposal\Text Summarization Code\summy.py";
            string pythonModelPickerPath = @"D:\MASTER WORK\After Proposal\Text Summarization Code\picker.py";

            ThesisDocumentDbContext thesisDocumentDbContext = new ThesisDocumentDbContext();
          
            thesisDocumentDbContext.Database.EnsureCreated();
            Console.WriteLine("Pass The pdf thesis documents Directory");
            string DirectoryPath = Console.ReadLine();
            Console.WriteLine("Load from \n 1-Only have PDFs and want to process them online \n 2-Having TEI files Locally");
            int option = int.Parse(Console.ReadLine());

            foreach (var documentPath in Directory.GetFiles(DirectoryPath))
            {
                FullTextProcessingUnit fullTextProcessingUnit = new FullTextProcessingUnit();
                FutueWorkSummarizer futueWorkSummarizer = new FutueWorkSummarizer(pythonExe, pythonFutureSummarizerPath);
                ModelPicker modelPicker = new ModelPicker(pythonExe, pythonModelPickerPath);
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
                    Title = fullTextProcessingUnit.GetThesisDocunetTitle(),
                    FutureWork = fullTextProcessingUnit.GetFutureWorkInfo(),
                    PublishedDate = fullTextProcessingUnit.GetPublishedDate(),
                    Abstract = fullTextProcessingUnit.GetAbstractInfo(),
                };
                if (thesis.Abstract == string.Empty && thesis.Title == string.Empty)
                {
                    Failed++;
                    Console.WriteLine("Failed");
                    continue;
                }
                try
                {
                    SummarizationResult summarizationResult = await futueWorkSummarizer.GetSummarizationResultAsync(thesis.FutureWork);
                    if(string.IsNullOrWhiteSpace(summarizationResult.SCIBert) || string.IsNullOrWhiteSpace(summarizationResult.RoBERTa))
                    {
                        Failed++;
                        Console.WriteLine("Failed");
                        continue;
                    }
                    PickerResult pickerResult = await modelPicker.GetModelAsync(thesis.Abstract == string.Empty ? thesis.Title : thesis.Abstract, summarizationResult.SCIBert, summarizationResult.RoBERTa);
                    if (string.IsNullOrWhiteSpace(pickerResult.Model) || string.IsNullOrWhiteSpace(pickerResult.Keywords))
                    {
                        Failed++;
                        Console.WriteLine("Failed");
                        continue;
                    }
                    thesis.SummarizedThesis = new SummarizedThesis
                    {
                        ModelName = pickerResult.Model,
                        Summarization = pickerResult.Model == "RoBERTa" ? summarizationResult.RoBERTa : summarizationResult.SCIBert,
                        Keywords = pickerResult.Keywords,
                    };
                    thesisDocumentDbContext.Theses.Add(thesis);
                    await thesisDocumentDbContext.SaveChangesAsync();
                    Correct++;
                    Console.WriteLine(Correct);
                }
                catch(Exception ex)
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