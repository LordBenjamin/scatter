using System;
using System.IO;
using SilentOrbit.Scatter.Data;

namespace SilentOrbit.Scatter.Templates
{
	class SiteIndexTemplate : Template
	{
		public SiteIndexTemplate(Site site) : base(site, "siteindex.html")
		{
		}

		public TemplateInstance Create(Site site)
		{
			var t = new TemplateInstance(this);
			t.LastModified = site.LastModified;
			t["site:path"] = site.UrlPath;
			t["site:title"] = site.Title;
			return t;
		}
	}
}

