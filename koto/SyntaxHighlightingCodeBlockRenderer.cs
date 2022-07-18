using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Text;

namespace Markdig.SyntaxHighlighting.Core
{
	public class SyntaxHighlightingCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
	{
		private readonly CodeBlockRenderer _underlyingRenderer;

		public SyntaxHighlightingCodeBlockRenderer(CodeBlockRenderer? underlyingRenderer = null)
		{
			_underlyingRenderer = underlyingRenderer ?? new CodeBlockRenderer();
		}

        protected override void Write(HtmlRenderer renderer, CodeBlock obj)
        {
            var fencedCodeBlock = obj as FencedCodeBlock;
            var parser = obj.Parser as FencedCodeBlockParser;
            if (fencedCodeBlock == null || parser == null)
            {
                _underlyingRenderer.Write(renderer, obj);
                return;
            }

            string? infoPrefix = parser.InfoPrefix;
            if (string.IsNullOrEmpty(infoPrefix))
                infoPrefix = string.Empty;
            string? languageMoniker = fencedCodeBlock.Info;
            if (!string.IsNullOrEmpty(languageMoniker))
                languageMoniker.Replace(infoPrefix, string.Empty);
            if (string.IsNullOrEmpty(languageMoniker))
            {
                _underlyingRenderer.Write(renderer, obj);
                return;
            }

            var code = GetCode(obj);

            renderer.WriteLine("<div class=\"codeblock\"><pre class=\"hl\">");
            var markup = ApplySyntaxHighlighting(languageMoniker, code);
            renderer.WriteLine(markup);
            renderer.WriteLine("</pre></div>");
        }

        private string ApplySyntaxHighlighting(string languageMoniker, string code)
        {
            return koto.Highlighter.HighlightCode(code, languageMoniker);
        }

        private static string GetCode(LeafBlock obj)
        {
            var code = new StringBuilder();
            foreach (var line in obj.Lines.Lines)
            {
                var slice = line.Slice;
                if (slice.Text == null)
                {
                    continue;
                }
                var lineText = slice.Text.Substring(slice.Start, slice.Length);
                code.AppendLine(lineText);
            }
            return code.ToString();
        }
    }
}
