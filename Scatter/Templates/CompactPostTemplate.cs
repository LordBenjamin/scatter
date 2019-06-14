using Markdig;
using SilentOrbit.Scatter.Data;

namespace SilentOrbit.Scatter.Templates {
    class CompactPostTemplate : Template
	{
		public CompactPostTemplate(GeneratorContext context) : base(context, "BlogPostCompact.html")
		{
		}

		public Html Generate(Site site, Post post)
		{
			var t = new TemplateInstance(this);
			t.LastModified = post.LastModified;
			t["title"] = post.Title;
			t["gmdate"] = post.Date.ToUniversalTime().ToString("yyyy-MM-dd\\THH:mm:ss\\Z");
			t["longdate"] = post.Date.ToString("ddd dd MMM yyyy");
			t["monthyear"] = post.Date.ToString("MMM yyyy");
			t["updated"] = post.LastModified.ToString("ddd dd MMM yyyy");
			t["url"] = site.UrlPath + post.Path;
			t["excerpt"] = Html.Raw(Markdown.ToHtml(post.ExcerptMarkdown, Context.Pipeline));
			t["title"] = post.Title;
			return t.ToHtml();
		}
	}
}

