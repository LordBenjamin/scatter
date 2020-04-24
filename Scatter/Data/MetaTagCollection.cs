using SilentOrbit.Scatter.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SilentOrbit.Scatter.Data
{
    internal class MetaTagCollection : List<MetaTag>
    {
        public MetaTagCollection()
        {
        }

        public MetaTagCollection(IEnumerable<MetaTag> collection) : base(collection)
        {
        }

        public MetaTagCollection(int capacity) : base(capacity)
        {
        }

        public string ToHtml(TemplateInstance template)
        {
            return string.Join("\n", this.Select(i => i.ToHtml(template)));
        }
    }
}
