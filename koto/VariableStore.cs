using System.Globalization;
using Markdig;

namespace koto
{
	internal class VariableStore
	{
		private static Variables GlobalVars = new();
		public static Variables LocalVars = new();
		public static Variables TempVars = new();
		public static Variables ConfigVars = new();
		private static DateTime buildDT = DateTime.Now;
		private static MarkdownPipeline? pipeline;

		public static void Init(MarkdownPipeline _pipeline)
		{
			pipeline = _pipeline;

			GlobalVars.Add("markdown", (s) => Var_markdown(s));
			GlobalVars.Add("page_title", (s) => Var_page_title(s));   // default page title

			GlobalVars.Add("filelist", (s) => Var_filelist(s));

			GlobalVars.Add("source_file", (s) => Path.GetFileName(s));
			GlobalVars.Add("source_file_path", (s) => s);
			GlobalVars.Add("source_file_full_path", (s) => Path.GetFullPath(s));

			GlobalVars.Add("modified_time_utc_iso", (s) => File.GetLastWriteTimeUtc(s).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
			GlobalVars.Add("modified_time_iso", (s) => File.GetLastWriteTime(s).ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
			GlobalVars.Add("modified_time_utc", (s) => File.GetLastWriteTimeUtc(s).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
			GlobalVars.Add("modified_time", (s) => File.GetLastWriteTime(s).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

			GlobalVars.Add("creation_time_utc_iso", (s) => File.GetCreationTimeUtc(s).ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
			GlobalVars.Add("creation_time_iso", (s) => File.GetCreationTime(s).ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
			GlobalVars.Add("creation_time_utc", (s) => File.GetCreationTimeUtc(s).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
			GlobalVars.Add("creation_time", (s) => File.GetCreationTime(s).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));

			GlobalVars.Add("build_time_utc_iso", (s) => buildDT.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
			GlobalVars.Add("build_time_iso", (s) => buildDT.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
			GlobalVars.Add("build_time_utc", (s) => buildDT.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
			GlobalVars.Add("build_time", (s) => buildDT.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
		}

		#region list management

		public static void AddGlobal(string name, string value)
		{
			GlobalVars.Add(name, (s) => value);
		}
		public static void AddLocal(string name, string value)
		{
			LocalVars.Add(name, (s) => value);
		}
		public static void AddTemp(string name, string value)
		{
			TempVars.Add(name, (s) => value);
		}
		public static void AddConfig(string name, string value)
		{
			ConfigVars.Add(name, (s) => value);
		}

		public static void ClearLocal()
		{
			LocalVars.Clear();
		}
		public static void ClearTemp()
		{
			TempVars.Clear();
		}
		public static void ClearConfig()
		{
			ConfigVars.Clear();
		}

		public static string Get(string name, string fileName)
		{
			if (TempVars.Exists(name))
				return TempVars.Get(name, fileName);
			if (ConfigVars.Exists(name))
				return ConfigVars.Get(name, fileName);
			if (GlobalVars.Exists(name))
				return GlobalVars.Get(name, fileName);
			throw new Exception("Unknown variable \"" + name + "\"");
		}

		public static bool Exists(string name)
		{
			return LocalVars.Exists(name) || GlobalVars.Exists(name) || ConfigVars.Exists(name);
		}

		#endregion


		#region variable delegates

		private static string Var_page_title(string fileName)
		{
			if (LocalVars.Exists("page_title"))
				return LocalVars.Get("page_title", fileName);
			return Path.GetFileNameWithoutExtension(fileName);
		}

		private static string Var_markdown(string fileName)
		{
			string? md = MarkdownReader.ReadMarkdown(fileName);
			if (md == null)
				throw new Exception("No markdown found in \"" + fileName + "\"");
			md = Template.Substitute(md, fileName);
			return Markdown.ToHtml(md, pipeline);
		}

		private static string filelist_page_title(string file)
		{
			if (TempVars.Exists("page_title"))
				return TempVars.Get("page_title", file);
			return Path.GetFileNameWithoutExtension(file);
		}

		private static string Var_filelist(string fileName)
		{
			int fileDepth = FileList.GetPathDepth(fileName);
			string html = string.Empty;
			html += "<div class=\"filelist_div\">\n";
			string? sectionName = string.Empty;
			string? printedSectionName = string.Empty;
			foreach (string file in FileList.MarkdownFiles)
			{
				int linkDepth = FileList.GetPathDepth(file);

				string? dirName = Path.GetDirectoryName(Path.GetRelativePath(Program.baseDir, file));
				if (dirName != sectionName)	// new section?
				{
					// get directory name
					sectionName = dirName;
					printedSectionName = sectionName;

					string dirNameFile = Program.baseDir + dirName + Path.DirectorySeparatorChar + ".dir_name.txt";
					if (File.Exists(dirNameFile))
					{
						string? newDirName;
						using (TextReader tr = new StreamReader(dirNameFile))
						{
							newDirName = tr.ReadLine();
						}
						if (!string.IsNullOrEmpty(newDirName))
							newDirName = newDirName.Trim();
						if (!string.IsNullOrEmpty(newDirName))
							printedSectionName = newDirName;
					}
					html += "<span class=\"filelist_section\">";

					int ld2 = linkDepth - 1;
					if (ld2 < 0)
						ld2 = 0;
					html += "<span class=\"filelist_depth_" + ld2.ToString() + "\">" + printedSectionName + "</span></span>\n";
				}

				// link
				string rp = Path.GetRelativePath(Path.GetFullPath(fileName),
												 Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(file) + ".html");
				if (rp == ".")  // link to self
					rp = Path.GetFileNameWithoutExtension(file) + ".html";
				else if (rp.StartsWith("..\\"))
					rp = rp.Substring(3);
				rp = rp.Replace('\\', '/');
				html += "<a href=\"" + rp + "\">";

				// inner HTML
				TempVars.Clear();
				MarkdownReader.ReadVars(file, TempVars);
				if (file == fileName)
					html += "<span class=\"filelist_self\">";
				else
					html += "<span class=\"filelist_document\">";

				html += "<span class=\"filelist_depth_" + linkDepth.ToString() + "\">";

				html += filelist_page_title(file);
				html += "</span></span></a>\n";
			}
			TempVars.Clear();
			html += "</div>";
			return html;
		}

		#endregion
	}
}
