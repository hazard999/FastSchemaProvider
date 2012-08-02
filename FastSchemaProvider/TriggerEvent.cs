using System;
using System.Collections.Generic;
using System.Linq;

namespace FastSchemaProvider
{
    [Flags]
    public enum TriggerEvent
    {
        Insert = 1,
        Update = 2,
        Delete = 4
    }
}
