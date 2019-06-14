using System;
using System.IO;
using Markdig;
using Markdig.SyntaxHighlighting;
using SilentOrbit.Scatter.Data;

namespace SilentOrbit.Scatter
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			try
			{
				string path = Environment.CurrentDirectory;
                if (args.Length > 0) {
                    path = Path.GetFullPath(Path.Combine(path, args[0]));
                }

                var pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .UseSyntaxHighlighting()
                    .Build();

                Site site = new Site(path);

                GeneratorContext context = new GeneratorContext(site, pipeline);

                Console.WriteLine("Generating site: " + site.Title);
				Generator.Generate(context);
				Console.WriteLine("All done");
				return 0;
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Error:");
				Console.Error.WriteLine(e.Message);
				Console.Error.WriteLine(e.StackTrace);
				if (e.InnerException != null)
					Console.Error.WriteLine(e.InnerException.Message);
				return -1;
			}
		}
	}
}
