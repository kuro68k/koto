using System.Diagnostics;
using System.Text;

namespace koto
{
	internal class Highlighter
	{
		private static string RunHighlighter(string arguments, string stdin)
		{
			Process process = new Process();
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.FileName = VariableStore.ConfigVars.Get("highlighter_exe");
			process.StartInfo.Arguments = arguments;
			process.StartInfo.RedirectStandardInput = true;
			process.StartInfo.RedirectStandardOutput = true;

			StringBuilder output = new();
			process.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
			{
				if (!String.IsNullOrEmpty(e.Data))
				{
					output.AppendLine(e.Data);
				}
			});

			process.Start();
			process.BeginOutputReadLine();

			if (!string.IsNullOrEmpty(stdin))
			{
				process.StandardInput.Write(stdin);
				process.StandardInput.Write("\u001A");	// EOF
			}

			process.WaitForExit();
			return output.ToString().Trim();
		}

		public static void GenerateCSSFile()
		{
			string cssFile = VariableStore.ConfigVars.Get("highlighter_css_output");
			string theme = VariableStore.ConfigVars.Get("highlighter_theme");
			string outFile = Program.baseDir + VariableStore.ConfigVars.Get("highlighter_css_output");
			outFile = Path.GetFullPath(outFile);    // normalizes directory separators
			outFile = FileList.GetOutputFileName(outFile);
			string outDir = Path.GetDirectoryName(outFile);
			if (!string.IsNullOrEmpty(outDir))
				Directory.CreateDirectory(outDir);
			string css = RunHighlighter("--print-style --stdout -s \"" + VariableStore.ConfigVars.Get("highlighter_theme") + "\"", string.Empty);
			using (StreamWriter sw = new StreamWriter(outFile))
			{
				sw.Write(css);
			}
		}

		public static string HighlightCode(string code, string langauge)
		{
			string args = VariableStore.ConfigVars.Get("highlighter_options");
			if (!string.IsNullOrEmpty(langauge))
				args += " -S " + langauge;
			args += " -O html -f -";
			return RunHighlighter(args, code);
		}
	}
}
