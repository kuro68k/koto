using Markdig;
using System.Text;

namespace koto
{
	internal class Template
	{
		string RawText = string.Empty;

		public Template(string baseDir)
		{
			Load(baseDir);
		}

		public void Load(string baseDir)
		{
			string filename = baseDir + @".koto\template.html";
			RawText = File.ReadAllText(filename);
			if (string.IsNullOrEmpty(RawText))
				throw new Exception("Template file \"" + filename + "\"is empty.");
		}

		public static string Substitute(string text, string fileName)
		{
			StringBuilder output = new();
			int idx = 0;
			int dollarCount = 0;
			int subStartIndex = -1;
			while (idx < text.Length)
			{
				char c = text[idx];
				if (c == '$')
				{
					dollarCount++;
					if (dollarCount == 1)
						subStartIndex = idx;
					if (dollarCount == 4)
					{
						if (subStartIndex == -1)
							throw new Exception("subStartIndex = -1.");
						if (idx - subStartIndex > 4)    // minimum 1 character variable name
						{
							output.Append(VariableStore.Get(text.Substring(subStartIndex + 2, idx - subStartIndex - 3), fileName));
						}
						else
							output.Append(text.Substring(subStartIndex, idx));
						subStartIndex = -1;
						dollarCount = 0;
					}
				}
				else if (char.IsWhiteSpace(c))  // cancel dollar count
				{
					if (subStartIndex != -1)
					{
						string debug = text.Substring(subStartIndex, idx - subStartIndex);
						output.Remove(output.Length - debug.Length + 1, debug.Length - 1);
						output.Append(text.Substring(subStartIndex, idx - subStartIndex));
					}
					dollarCount = 0;
					subStartIndex = -1;
					output.Append(c);
				}
				else
				{
					if (dollarCount != 2)   // don't write the variable name
						output.Append(c);
				}
				idx++;
			}
			return output.ToString();
		}

		public void MarkdownToHTML(string fileName, MarkdownPipeline pipeline)
		{
			string outputFilename = FileList.GetOutputFileName(fileName);
			outputFilename = Path.GetDirectoryName(outputFilename) +
							 Path.DirectorySeparatorChar +
							 Path.GetFileNameWithoutExtension(outputFilename) + ".html";
			Console.WriteLine("\"" + Path.GetRelativePath(Program.baseDir, fileName) + "\" -> \"" + Path.GetRelativePath(Program.baseDir, outputFilename) + "\"");
			string? outputDir = Path.GetDirectoryName(outputFilename);
			if (!string.IsNullOrEmpty(outputDir))
				Directory.CreateDirectory(outputDir);
			using (TextWriter tw = new StreamWriter(outputFilename))
			{
				tw.Write(Substitute(RawText, fileName));
			}
		}
	}
}
