using System;
using System.Collections.Generic;
using System.Linq;

namespace FastSchemaProvider
{
    public class Table
    {
        public Table()
        {
            ForeignKeys = new List<ForeignKey>();
            Columns = new List<Column>();
            Trigger = new List<Trigger>();
            Indizes = new List<Index>();
        }

        public Table(string actualname) :
            this()
        {
            ActualName = actualname;
        }

        public string ActualName { get; set; }

        public Key PrimaryKey { get; set; }

        public List<ForeignKey> ForeignKeys { get; set; }

        public List<Index> Indizes { get; set; }

        public List<Column> Columns { get; set; }

        public List<Trigger> Trigger { get; set; }

        public void BuildPrimaryKey()
        {
            PrimaryKey = new Key();

            Columns.ForEach(col =>
            {
                if (col.IsPrimaryKeyColumn)
                    PrimaryKey.Columns.Add(col.ActualName);
            });
        }
    }
}
