using Markdig;
using SilentOrbit.Scatter.Data;
using System;

namespace SilentOrbit.Scatter
{
    internal class GeneratorContext
    {
        public GeneratorContext(Site site, MarkdownPipeline pipeline)
        {
            Site = site ?? throw new ArgumentNullException(nameof(site));
            Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        public Site Site { get; }

        public MarkdownPipeline Pipeline { get; }
    }
}
