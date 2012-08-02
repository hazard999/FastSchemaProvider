using System;

namespace FastSchemaProvider
{
    public class IndexColumn
    {
        public string ColumnName { get; set; }
        public int Sequence { get; set; }
        public Ordering Order { get; set; }	
    }
}
