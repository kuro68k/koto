using Markdig;

namespace koto
{
	internal class Program
	{
		public static string baseDir = string.Empty;
		public static string outputDir = string.Empty;
		public static string kotoDir = string.Empty;

		static void Main(string[] args)
		{
			CmdArgs argProcessor = new CmdArgs(
				new List<CmdArgument> {
					{ new CmdArgument(ArgType.String,
								required: true,
								short_description: "site_directory",
								assign: (dynamic d) => { baseDir = (string)d; }) },
					}
				);

			string[]? remainder;
			if (!argProcessor.TryParse(args, out remainder))
			{
				//argProcessor.PrintHelp();
				Console.WriteLine("koto <site_directory>");
				return;
			}
			baseDir = Path.GetFullPath(baseDir);
			if (!baseDir.EndsWith(Path.DirectorySeparatorChar))
				baseDir += Path.DirectorySeparatorChar;
			
			outputDir = baseDir + @".output";
			if (!outputDir.EndsWith(Path.DirectorySeparatorChar))
				outputDir += Path.DirectorySeparatorChar;

			kotoDir = baseDir + @".koto";
			if (!kotoDir.EndsWith(Path.DirectorySeparatorChar))
				kotoDir += Path.DirectorySeparatorChar;

			// read config
			VariableStore.ClearConfig();
			MarkdownReader.ReadVars(kotoDir + "config.txt", VariableStore.ConfigVars);

			var pipeline = new MarkdownPipelineBuilder()
								.UseAdvancedExtensions()
								.Build();
			pipeline.Extensions.Add(new Markdig.SyntaxHighlighting.Core.SyntaxHighlightingExtension());
			VariableStore.Init(pipeline);
			VariableStore.AddGlobal("output_dir", outputDir);
			var template = new Template(baseDir);

			Console.WriteLine("Cleaning output directory...");
			FileList.CleanDir(outputDir);

			if (!FileList.Scan())
			{
				Console.WriteLine("No markdown files found to process.");
				return;
			}

			foreach (string fileName in FileList.MarkdownFiles)
			{
				VariableStore.ClearLocal();
				MarkdownReader.ReadVars(fileName, VariableStore.LocalVars);
				template.MarkdownToHTML(fileName, pipeline);
			}

			FileList.CopyOthersToOutput();

			// code syntax highlighting
			if (VariableStore.ConfigVars.Exists("highlighter_css_output"))
			{
				Highlighter.GenerateCSSFile();
			}
		}
	}
}