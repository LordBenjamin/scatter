using Markdig;
using SilentOrbit.Scatter.Data;

namespace SilentOrbit.Scatter.Templates {
    class PageTemplate : Template
	{
		public PageTemplate(GeneratorContext context) : base(context, "Page.html")
		{

		}

		public Html Generate(Site site, Page page)
		{
			var t = new TemplateInstance(this);
			t.LastModified = page.LastModified;
			t["title"] = page.Title;
			t["contents"] = Html.Raw(Markdown.ToHtml(page.Content, Context.Pipeline));
			return t.ToHtml();
		}
	}
}

