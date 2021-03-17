using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorTable.Components.ServerSide
{
    public class PaginationResult<T>
    {
        public int Top { get; set; }

        public int Skip { get; set; }

        public int? Total { get; set; }

        public IReadOnlyCollection<T> Records { get; set; }
    }
}
