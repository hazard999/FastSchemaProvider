using System;
using System.Collections.Generic;

namespace FastSchemaProvider
{
    public class Index
    {
        public Index()
        {
            Columns = new List<IndexColumn>();
        }

        public string IndexName { get; set; }
        public IndexTypes IndexType { get; set; }
        public IList<IndexColumn> Columns { get; set; }
    }
}
