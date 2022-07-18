namespace koto
{
	internal class MarkdownReader
	{
		public static void ReadVars(string fileName, Variables vars)
		{
			using (TextReader tr = new StreamReader(fileName))
			{
				int lineno = 1;
				string? line;
				while ((line = tr.ReadLine()) != null)
				{
					if (line.StartsWith("---"))     // end of variables section
						break;
					line = line.Trim();
					if (string.IsNullOrEmpty(line))
						continue;
					if (line.StartsWith('#'))       // comment
						continue;
					if (!line.Contains('='))
						throw new Exception("Syntax error on line " + lineno.ToString() + " in file \"" + Path.GetRelativePath(Program.baseDir, fileName) + "\"");
					int idx = line.IndexOf('=');
					if (idx == 0)
						throw new Exception("Syntax error on line " + lineno.ToString() + " in file \"" + Path.GetRelativePath(Program.baseDir, fileName) + "\"");
					string name = line.Substring(0, idx - 1).Trim();
					string value = line.Substring(idx + 1).Trim();
					if (line.EndsWith('\\'))
					{
						while ((line = tr.ReadLine()) != null)
						{
							line = line.Trim();
							value += " " + line;
							if (!line.EndsWith('\\'))
								break;
						}
					}
					vars.Add(name, (s) => value);
				}
			}
		}

		public static string? ReadMarkdown(string fileName)
		{
			using (TextReader tr = new StreamReader(fileName))
			{
				string? line;
				while ((line = tr.ReadLine()) != null)
				{
					if (line.StartsWith("---"))     // end of variables section
						break;
					line = line.Trim();
					if (string.IsNullOrEmpty(line))
						continue;
				}
				return tr.ReadToEnd();
			}
		}
	}
}
