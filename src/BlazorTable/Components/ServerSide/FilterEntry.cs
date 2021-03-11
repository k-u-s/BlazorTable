using System.Collections.Generic;

namespace BlazorTable.Components.ServerSide
{
    public class FilterEntry
    {
        public virtual string Key { get; set; }
        
        public virtual string Source { get; set; }
        
        public virtual string Condition { get; set; }

        public virtual IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}