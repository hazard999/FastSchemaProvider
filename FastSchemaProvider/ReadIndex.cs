using System;

namespace FastSchemaProvider
{
    internal class ReadIndex
    {
        public string TableName { get; set; }
        public string IndexName { get; set; }
        public IndexTypes IndexType { get; set; }
        public string ColumnName { get; set; }
        public int Sequence { get; set; }	
        public Ordering Order { get; set; }	
    }
}
