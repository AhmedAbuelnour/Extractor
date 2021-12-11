using System.Diagnostics;
using System.Text.Json;

namespace PretrainedModelPicker
{
    public class ModelPicker
    {
        public string PythonExecutablePath { get; private set; }
        public string ScriptPath { get; private set; }

        public ModelPicker(string pythonExecutablePath, string scriptPath)
        {
            PythonExecutablePath = pythonExecutablePath;
            ScriptPath = scriptPath;
        }
        public async Task<PickerResult> GetModelAsync(string Abstract, string SCIBERT, string RoBERTa)
        {
            ProcessStartInfo psi = new()
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
            return JsonSerializer.Deserialize<PickerResult>(await reader.ReadToEndAsync());
        }
    }
}