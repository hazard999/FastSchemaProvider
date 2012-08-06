using System;
using System.Collections.Generic;

namespace FastSchemaProvider
{
    public class SchemaDiff
    {
        public SchemaDiff() { }

        public IEnumerable<Table> TablesToDrop { get; set; }
        public IEnumerable<Table> TablesToCreate { get; set; }
    }
}
