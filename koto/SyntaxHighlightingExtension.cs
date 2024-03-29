﻿using System;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Markdig.SyntaxHighlighting.Core
{
	public class SyntaxHighlightingExtension : IMarkdownExtension
	{
        public void Setup(MarkdownPipelineBuilder pipeline) { }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            var htmlRenderer = renderer as TextRendererBase<HtmlRenderer>;
            if (htmlRenderer == null)
            {
                return;
            }

            var originalCodeBlockRenderer = htmlRenderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
            if (originalCodeBlockRenderer != null)
            {
                htmlRenderer.ObjectRenderers.Remove(originalCodeBlockRenderer);
            }

            htmlRenderer.ObjectRenderers.AddIfNotAlready(
               new SyntaxHighlightingCodeBlockRenderer(originalCodeBlockRenderer));
        }
    }
}
