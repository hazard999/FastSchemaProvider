using System;
using System.Collections.Generic;

namespace FastSchemaProvider
{
    public class Trigger
    {
        private const string InsertConst = "insert";
        private const string UpdateConst = "update";
        private const string DeleteConst = "delete";
        private const string BeforeConst = "before";
        private const string AfterConst = "after";

        public string Name { get; set; }
        public string Table { get; set; }
        public TriggerEvent Event { get; set; }
        public TriggerTime Time { get; set; }
        public string Definition { get; set; }

        public string EventAsString
        {
            get
            {
                IList<string> result = new List<string>();

                if (Event.HasFlag(TriggerEvent.Insert))
                    result.Add(InsertConst);

                if (Event.HasFlag(TriggerEvent.Update))
                    result.Add(UpdateConst);

                if (Event.HasFlag(TriggerEvent.Delete))
                    result.Add(DeleteConst);

                return String.Join(",", result);
            }
            set
            {
                if (value.ToLowerInvariant().Contains(InsertConst))
                    Event |= TriggerEvent.Insert;

                if (value.ToLowerInvariant().Contains(UpdateConst))
                    Event |= TriggerEvent.Update;

                if (value.ToLowerInvariant().Contains(DeleteConst))
                    Event |= TriggerEvent.Delete;
            }
        }

        public string TimeAsString
        {
            get
            {
                if (Time == TriggerTime.Before)
                    return BeforeConst;

                return AfterConst;
            }
            set
            {
                if (value.ToLowerInvariant() == BeforeConst)
                    Time = TriggerTime.Before;

                if (value.ToLowerInvariant() == AfterConst)
                    Time = TriggerTime.After;
            }
        }
    }
}
