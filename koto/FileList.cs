namespace koto
{
	internal class FileList
	{
		public static List<string> MarkdownFiles = new List<string>();
		public static List<string> OtherFiles = new List<string>();

		public static bool Scan()
		{
			MarkdownFiles.Clear();
			ScanDir(Program.baseDir);
			return MarkdownFiles.Count > 0;
		}

		private static void ScanDir(string dir)
		{
			string[] list = Directory.GetFiles(dir);
			foreach (string name in list)
			{
				if (Path.GetFileName(name).StartsWith('.'))
					continue;
				if (name.EndsWith(".md"))
					MarkdownFiles.Add(name);
				else
					OtherFiles.Add(name);
			}
			list = Directory.GetDirectories(dir);
			foreach (string name in list)
			{
				string dirName = name.Replace(Path.GetDirectoryName(name) + Path.DirectorySeparatorChar, "");
				if (!dirName.StartsWith('.'))
					ScanDir(name);
			}
		}

		public static void CleanDir(string dir)
		{
			string[] list = Directory.GetFiles(dir);
			foreach (string name in list)
				File.Delete(name);
			list = Directory.GetDirectories(dir);
			foreach (string name in list)
			{
				CleanDir(name);
				Directory.Delete(name);
			}
		}

		public static void CopyOthersToOutput()
		{
			foreach (string file in OtherFiles)
			{
				string outputFile = GetOutputDirectory(file) + Path.GetFileName(file);
				string? dir = Path.GetDirectoryName(outputFile);
				if (!string.IsNullOrEmpty(dir))
					Directory.CreateDirectory(dir);
				File.Copy(file, outputFile, true);
			}
		}

		
		#region utilities

		public static string GetOutputDirectory(string fileName)
		{
			string outputDir = Program.outputDir + Path.GetDirectoryName(Path.GetRelativePath(Program.baseDir, fileName));
			if (!outputDir.EndsWith(Path.DirectorySeparatorChar))
				outputDir += Path.DirectorySeparatorChar;
			return outputDir;
		}

		public static string GetRelativeDirectory(string path)
		{
			string? rel = Path.GetDirectoryName(Path.GetRelativePath(Program.baseDir, path));
			if (rel == null)
				rel = string.Empty;
			return rel;
		}

		public static string GetOutputFileName(string fileName)
		{
			return GetOutputDirectory(fileName) + Path.GetFileName(fileName);
		}

		public static int GetPathDepth(string fileName)
		{
			fileName = Path.GetRelativePath(Program.baseDir, fileName);
			return fileName.Split(Path.DirectorySeparatorChar).Length - 1;
		}

		#endregion
	}
}
