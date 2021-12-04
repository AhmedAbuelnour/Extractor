using System.Diagnostics;
using System.Text.Json;

namespace Summarizer
{
    public class FutueWorkSummarizer
    {
        public string PythonExecutablePath { get; private set; }
        public string ScriptPath { get; private set; }
        public FutueWorkSummarizer(string pythonExecutablePath, string scriptPath)
        {
            PythonExecutablePath = pythonExecutablePath ?? "d:/MASTER WORK/After Proposal/Text Summarization Code/venv/Scripts/python.exe";
            ScriptPath = scriptPath ?? "d:/MASTER WORK/After Proposal/Text Summarization Code/summarizer.py";
        }

        public async Task<SummarizationResult> GetSummarizationResultAsync(string FutureWork)
        {
            ProcessStartInfo psi = new()
            {
                FileName = PythonExecutablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = string.Format("\"{0}\" \"{1}\"", ScriptPath, FutureWork)
            };
            using Process process = Process.Start(psi);
            using StreamReader reader = process.StandardOutput;
            return JsonSerializer.Deserialize<SummarizationResult>(await reader.ReadToEndAsync());
        }
    }
}
