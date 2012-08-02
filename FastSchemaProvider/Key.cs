using System;
using System.Collections.Generic;
using System.Linq;

namespace FastSchemaProvider
{
    public class Key
    {
        public Key()
        {
            Columns = new List<string>();
        }

        public List<string> Columns { get; set; }
    }
}
