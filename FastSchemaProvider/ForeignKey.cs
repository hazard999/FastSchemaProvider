using System;
using System.Collections.Generic;
using System.Linq;

namespace FastSchemaProvider
{
    public class ForeignKey
    {
        public ForeignKey()
        {
            Columns = new List<string>();
            UniqueColumns = new List<string>();
        }

        public ForeignKey(string detailTable, string name, IEnumerable<string> columns, string masterTable, IEnumerable<string> masterColumns, string updateAction, string deleteAction, bool checkOnCommit)
            : this()
        {
            Name = name;
            Columns.AddRange(columns);
            DetailTable = detailTable;
            MasterTable = masterTable;
            UniqueColumns.AddRange(masterColumns);

            UpdateAction = updateAction;
            DeleteAction = deleteAction;
            CheckOnCommit = checkOnCommit;
        }

        public string Name { get; set; }
        public string UpdateAction { get; set; }
        public string DeleteAction { get; set; }
        public bool CheckOnCommit { get; set; }	

        public List<string> Columns { get; set; }

        public string DetailTable { get; set; }
        public string MasterTable { get; set; }
        
        public List<string> UniqueColumns { get; set; }
    }
}
