using System.Collections.Generic;

namespace BlazorTable.Components.ServerSide
{
    public class SearchHintsResult
    {
        public IEnumerable<string> Records { get; set; }
    }
}