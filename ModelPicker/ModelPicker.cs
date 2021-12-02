using System.Diagnostics;

namespace PretrainedModelPicker
{
    public class ModelPicker
    {
        public string PythonExecutablePath { get; private set; }
        public string ScriptPath { get; private set; }

        public ModelPicker(string pythonExecutablePath, string scriptPath)
        {
            PythonExecutablePath = pythonExecutablePath ??= "d:/MASTER WORK/After Proposal/Text Summarization Code/venv/Scripts/python.exe";
            ScriptPath = scriptPath ??= "d:/MASTER WORK/After Proposal/Text Summarization Code/src.py";
        }
        public async Task<string> GetModelAsync(string Abstract,string SCIBERT, string RoBERTa)
        {
            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = PythonExecutablePath,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", ScriptPath, Abstract, SCIBERT, RoBERTa)
            };

            using Process process = Process.Start(psi);
            using StreamReader reader = process.StandardOutput;
            return await reader.ReadToEndAsync();
        }
    }
}