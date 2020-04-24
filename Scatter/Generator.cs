using System;
using System.IO;
using SilentOrbit.Scatter.Data;
using System.Text;
using SilentOrbit.Scatter.Templates;
using Markdig;

namespace SilentOrbit.Scatter
{
	static class Generator
	{
		public static void Generate(GeneratorContext context)
		{
			LoadData(context.Site);

            //Read templates
            var siteIndexTemplate = new SiteIndexTemplate(context);
            var indexTemplate = new IndexTemplate(context);

			//Clean web target
			FileManager.Clean(context.Site.WebPath);
			//Copy static: Done in calling bash script
			FileManager.Clone(context.Site.StaticPath, context.Site.WebPath);

            //Generate news
            siteIndexTemplate["site:news"] = GenerateNews(context.Site);
            indexTemplate["site:news"] = GenerateNews(context.Site);

            var pageTemplate = new PageTemplate(context);
			var postTemplate = new PostTemplate(context);
            
			//Generate all files
			GenerateIndex(siteIndexTemplate, context);
			GenerateFeed(context);
			foreach (Page p in context.Site.Pages)
				GeneratePage(context.Site, p, pageTemplate, indexTemplate);
			foreach (Post p in context.Site.Posts)
				GeneratePost(context.Site, p, postTemplate, indexTemplate);
			GenerateNewPostPageTemplate(context.Site);

			Compressor.CompressDirectoryRecursive(context.Site.WebPath);
		}

		static void LoadData(Site site)
		{
			//Scan for pages
			string[] pageFiles = Directory.GetFiles(site.DataPath, "*.page", SearchOption.AllDirectories);
			foreach (string pageFile in pageFiles)
			{
				Page p = new Page(site.DataPath, pageFile);
				//Ignore news pages
				if (p.HasDate)
					continue;

				site.Add(p);
				//Console.WriteLine("Page: " + p);
			}
			site.Pages.Sort(Page.ComparerIndex);

			//Scan for posts
			string[] postFiles = Directory.GetFiles(site.DataPath, "*.post", SearchOption.AllDirectories);
			foreach (string postFile in postFiles)
			{
				Post p = new Post(postFile);
				//Ignore non news pages
				if (p.Date == DateTime.MinValue)
					continue;

				site.Add(p);
				//Console.WriteLine("News: " + p);
			}
			site.Posts.Sort(Post.ComparerLatestFirst);
		}
        /// <summary>
        /// Helper for generating html list item <li></li>
        /// </summary>
		static Html LiTag(string url, string title, string text, bool active, DateTime modified)
		{
			Html titleHtml = new Html();
			if (title != "")
				titleHtml = Html.Raw(" title=\"") + title + Html.Raw("\"");
			if (active)
				return Html.Raw("<li><strong") + titleHtml + Html.Raw(">") + text + Html.Raw("</strong></li>", modified);
			else
				return Html.Raw("<li><a href=\"") + url + Html.Raw("\"") + titleHtml + Html.Raw(">") + text + Html.Raw("</a></li>", modified);
		}

		static Html GenerateTabs(Site site, Page page, Post post)
		{
			//Find out if we have an index page
			Page indexPage = null;
			foreach (Page p in site.Pages)
			{
				if (p.Name == "index")
				{
					indexPage = p;
					break;
				}
			}
			Html tabs = new Html();
			if (indexPage == null)
				tabs += LiTag(site.UrlPath, "", "News", page == null && post == null, DateTime.MinValue);
			else
				tabs += LiTag(site.UrlPath, indexPage.Title, indexPage.LinkTitle, page == indexPage, indexPage.LastModified);
			foreach (Page p in site.Pages)
			{
				if (p.LinkTitle == "")
					continue;
				if (p == indexPage)
					continue;
				tabs += LiTag(p.LinkUrl ?? site.UrlPath + p.Path, p.Title, p.LinkTitle, p == page, p.LastModified);
			}
			return tabs;
		}

		static Html GenerateNews(Site site)
		{
			Html news = Html.Raw("<ul>");
			string lastMonth = "";
			foreach (Post p in site.Posts)
			{
				string month = p.Date.ToString("MMM yyyy");
				if (month != lastMonth)
				{
					lastMonth = month;
					news += Html.Raw("</ul><h3>") + month + Html.Raw("</h3><ul>");
				}
				news += LiTag(site.UrlPath + p.Path, "", p.Title, false, p.LastModified);
			}
			news += Html.Raw("</ul>");
			return news;
		}

		static void GenerateIndex(SiteIndexTemplate template, GeneratorContext context)
		{
			var indexInstance = template.Create(context.Site);
			if (context.Site.GetPage("index") != null || File.Exists(Path.Combine(context.Site.StaticPath, "index.html")))
				return;

			var compactPostTemplate = new CompactPostTemplate(context);
			indexInstance["title"] = context.Site.Title;
			indexInstance["tabs"] = GenerateTabs(context.Site, null, null);
			indexInstance["contents"] = new Html();
			foreach (Post p in context.Site.Posts)
			{
				indexInstance["contents"] += compactPostTemplate.Generate(context.Site, p);
			}
			indexInstance.Write(Path.Combine(context.Site.WebPath, "index.html"));
		}

		static void GenerateFeed(GeneratorContext context)
		{
			string feedDir = Path.Combine(context.Site.WebPath, "feed");
			Directory.CreateDirectory(feedDir);
			using (TextWriter w = new StreamWriter(Path.Combine(feedDir, ".htaccess"), false, Encoding.ASCII))
			{
				w.WriteLine("DirectoryIndex index.atom");
				w.WriteLine("ForceType application/atom+xml");
				w.WriteLine("AddType application/atom+xml .atom");
			}
			var feedTemplate = new FeedTemplate(context);
			var entryTemplate = new FeedEntryTemplate(context);

			//At least 5 posts and one month
			Html entries = new Html();
			int posts = 0;
			DateTime expire = DateTime.Now.AddMonths(-1);
			foreach (Post p in context.Site.Posts)
			{
				if (posts > 5 && p.Date < expire && p.LastModified < expire)
					continue;

				entries += entryTemplate.Generate(context.Site, p);

				posts += 1;
			}
			feedTemplate.Generate(entries, Path.Combine(feedDir, "index.atom"));
		}

		static void GeneratePage(Site site, Page p, PageTemplate pageTemplate, IndexTemplate indexTemplate)
		{
			string dirPath = Path.Combine(site.WebPath, p.Path);
			if (p.Path == "index/")
				dirPath = site.WebPath;
			Directory.CreateDirectory(dirPath);
			if (Directory.Exists(p.SourceDir))
				FileManager.Clone(p.SourceDir, dirPath);

			var indexInstance = indexTemplate.Create(site);
			indexInstance["title"] = p.Title;
			indexInstance["tabs"] = GenerateTabs(site, p, null);
			indexInstance["contents"] = pageTemplate.Generate(site, p);
			indexInstance["metatags"] = Html.Raw(p.MetaTags.ToHtml(indexInstance));
			indexInstance.Write(Path.Combine(dirPath, "index.html"));
		}

		static void GeneratePost(Site site, Post p, PostTemplate postTemplate, IndexTemplate indexTemplate)
		{
			string dirPath = Path.Combine(site.WebPath, p.Path);
			string htmlPath = Path.Combine(dirPath, "index.html");
			Directory.CreateDirectory(dirPath);
			if (Directory.Exists(p.SourceDir))
				FileManager.Clone(p.SourceDir, dirPath);

			var inst = indexTemplate.Create(site);
			inst["title"] = p.Title;
			inst["author"] = p.Author;
			inst["date"] = p.Date.ToString("yyyy-MM-dd hh:mm:ss");
			inst["tabs"] = GenerateTabs(site, null, p);
			inst["contents"] = postTemplate.Generate(site, p);
			inst["metatags"] = Html.Raw(p.MetaTags.ToHtml(inst));
			inst.Write(htmlPath);
		}

		static void GenerateNewPostPageTemplate(Site site)
		{
			string postTemplatePath = Path.Combine(site.DataPath, "new.post-dist");
			using (TextWriter w = new StreamWriter(postTemplatePath, false, Encoding.UTF8))
			{
				w.WriteLine("Date: " + DateTime.Now.ToString("yyyy-MM-dd"));
				w.WriteLine("Author: ");
				w.WriteLine("Title: ");
				w.WriteLine();
				w.WriteLine("# ");
				w.WriteLine();
				w.WriteLine();
			}
			string pageTemplatePath = Path.Combine(site.DataPath, "new.page-dist");
			using (TextWriter w = new StreamWriter(pageTemplatePath, false, Encoding.UTF8))
			{
				w.WriteLine("Title: ");
				w.WriteLine("#LinkTitle: ");
				w.WriteLine("#LinkUrl: ");
				w.WriteLine("Index: ");
				w.WriteLine();
				w.WriteLine("# ");
				w.WriteLine();
				w.WriteLine();
			}
		}

		static DateTime Latest(params DateTime[] dates)
		{
			DateTime latest = DateTime.MinValue;
			foreach (DateTime d in dates)
			{
				if (d > latest)
					latest = d;
			}
			return latest;
		}
	}
}

