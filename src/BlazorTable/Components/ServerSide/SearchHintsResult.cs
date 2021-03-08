using System.Collections.Generic;

namespace BlazorTable.Components.ServerSide
{
    public class SearchHintsResult<T>
    {
        public int Top { get; set; }

        public int Skip { get; set; }

        public int? Total { get; set; }

        public IEnumerable<T> Records { get; set; }
    }
}