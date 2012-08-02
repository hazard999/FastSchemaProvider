using System;

namespace FastSchemaProvider
{
    public class ColumnToChange
    {
        public Table Table { get; set; }
        public Column NewColumnDefinition { get; set; }
        public Column OldColumnDefinition { get; set; }
    }
}
