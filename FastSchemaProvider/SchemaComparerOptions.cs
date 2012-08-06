using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FastSchemaProvider
{
    public class SchemaComparerOptions
    {
        public bool OldSchemaTreatsViewsAsTable { get; set; }
        public string OldSchemaTableAsViewPrefix { get; set; }
    }
}
