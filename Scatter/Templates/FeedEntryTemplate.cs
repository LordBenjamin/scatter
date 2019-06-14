using Markdig;
using SilentOrbit.Scatter.Data;

namespace SilentOrbit.Scatter.Templates {
    class FeedEntryTemplate : Template
	{
		public FeedEntryTemplate(Site site) : base(site, "feed.entry.xml")
		{
		}

		public Html Generate(Site site, Post post)
		{
			var t = new TemplateInstance(this);
			t.LastModified = post.LastModified;
			t["title"] = post.Title;
			t["author"] = post.Author;
			t["url"] = site.URL + post.Path;
			t["updated"] = post.LastModified.ToUniversalTime().ToString("yyyy-MM-dd\\THH:mm:ss\\Z");
			t["excerpt"] = Markdown.ToHtml(post.ExcerptMarkdown).Trim();
			t["content"] = Markdown.ToHtml(post.Content).Trim();
			return t.ToHtml();
		}
	}
}

