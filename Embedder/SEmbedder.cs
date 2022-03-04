using System.Diagnostics;

namespace Embedder
{
    public class SEmbedder
    {
        public string PythonExecutablePath { get; private set; }
        public string ScriptPath { get; private set; }

        public SEmbedder(string pythonExecutablePath, string scriptPath)
        {
            PythonExecutablePath = pythonExecutablePath;
            ScriptPath = scriptPath;
        }
        public async Task<string> GetEmbeddingAsync(string sentence)
        {
            ProcessStartInfo psi = new()
            {
                FileName = PythonExecutablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = string.Format("\"{0}\" \"{1}\"", ScriptPath, sentence)
            };
            using Process process = Process.Start(psi);
            using StreamReader reader = process.StandardOutput;
            return await reader.ReadToEndAsync();
        }
    }
}