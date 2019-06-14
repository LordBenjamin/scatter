using System;
using System.IO;
using SilentOrbit.Scatter.Data;

namespace SilentOrbit.Scatter.Templates
{
	class FeedTemplate : Template
	{
		public FeedTemplate(GeneratorContext context) : base(context, "feed.xml")
		{
		}

		public void Generate(Html entries, string path)
		{
			var t = new TemplateInstance(this);
			t.LastModified = Context.Site.LastModified;
			t["title"] = Context.Site.Title;
			t["url"] = Context.Site.URL;
			t["updated"] = t.LastModified.ToUniversalTime().ToString(@"yyyy-MM-dd\THH:mm:ss\Z");
			t["entries"] = entries;
			t.Write(path);
		}
	}
}

