using System;
using System.Collections.Generic;
using System.Linq;

namespace BlazorTable.Components.ServerSide
{
    public class FilterEntry
    {
        public virtual string Key { get; set; }
        
        public virtual string Source { get; set; }
        
        public virtual string Condition { get; set; }

        public virtual IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public override string ToString()
            => $"{Key}, {Source}, {Condition}{Environment.NewLine}" +
               $"{string.Join(Environment.NewLine, Parameters.Select(el => $"{el.Key}: {el.Value}"))}";
    }
}