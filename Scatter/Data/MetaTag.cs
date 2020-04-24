using SilentOrbit.Scatter.Templates;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilentOrbit.Scatter.Data
{
    internal class MetaTag
    {

        public MetaTag(string name, string content)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public string Name { get; }
        public string Content { get; }

        internal string ToHtml(TemplateInstance template)
        {
            string content = Content;

            foreach(var variable in template.vars)
            {
                content = content.Replace($"{{{variable.Key}}}", variable.Value.ToString());
            }

            return $"<meta name=\"{Html.Escape(Name)}\" content=\"{Html.Escape(content)}\" />";
        }

        internal static MetaTag ParseFrontMatter(string value)
        {
            bool isInsideQuote = false;

            StringBuilder nameBuilder = new StringBuilder(value.Length);
            StringBuilder valueBuilder = new StringBuilder(value.Length);

            StringBuilder builder = nameBuilder;

            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];

                if (current == '"')
                {
                    isInsideQuote = !isInsideQuote;
                    continue;
                }

                if (current == ':' && !isInsideQuote)
                {
                    builder = valueBuilder;
                    continue;
                }

                if((current == ' ' || current == '\t') && !isInsideQuote)
                {
                    continue;
                }

                builder.Append(current);
            }

            if (nameBuilder.Length == 0 || valueBuilder.Length == 0)
            {
                throw new InvalidOperationException();
            }

            return new MetaTag(nameBuilder.ToString(), valueBuilder.ToString());
        }
    }
}
