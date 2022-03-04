using System.Diagnostics;


namespace Embedder
{
    public class EmbeddingSimilarityCalculator
    {
        public string PythonExecutablePath { get; private set; }
        public string ScriptPath { get; private set; }

        public EmbeddingSimilarityCalculator(string pythonExecutablePath, string scriptPath)
        {
            PythonExecutablePath = pythonExecutablePath;
            ScriptPath = scriptPath;
        }
        public async Task<double> GetEmbeddingScoreAsync(string keywordEmbedding, string sentenceEmbedding)
        {
            ProcessStartInfo psi = new()
            {
                FileName = PythonExecutablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\"", ScriptPath, keywordEmbedding, sentenceEmbedding)
            };
            using Process process = Process.Start(psi);
            using StreamReader reader = process.StandardOutput;
            return double.Parse(await reader.ReadToEndAsync());
        }
    }
}
