using System.Collections.Generic;

namespace BlazorTable.Components.ServerSide
{
    public class SearchHints
    {
        public string Key { get; set; }
        
        public string Hint { get; set; }
        
        public int? Top { get; set; }

        public int? Skip { get; set; }
    }
}