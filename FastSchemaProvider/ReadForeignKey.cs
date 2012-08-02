using System;

namespace FastSchemaProvider
{
    internal class ReadForeignKey
    {
        public string TableName { get; set; }
        public string ConstraintName { get; set; }
        public string ColumnName { get; set; }
        public string UniqueTableName { get; set; }
        public string UniqueColumnName { get; set; }
        public string UpdateAction { get; set; }
        public string DeleteAction { get; set; }
        public bool CheckOnCommit { get; set; }	
    }
}
