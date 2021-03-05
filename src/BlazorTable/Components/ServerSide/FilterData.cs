using System.Collections.Generic;
using System.ComponentModel;

namespace BlazorTable.Components.ServerSide
{
    public class FilterData
    {
        public string OrderBy { get; set; }
        
        public SortDirection OrderDirection { get; set; }

        public string GlobalSearchQuery { get; set; }

        public Dictionary<string, FilterEntry> FilterEntries { get; set; }
        
        public int? Top { get; set; }

        public int? Skip { get; set; }
    }

    public enum SortDirection
    {
        /// <summary>
        /// Sort is not determined.
        /// </summary>
        UnSet,
        
        /// <summary>
        /// Sort in ascending order.
        /// </summary>
        Ascending,

        /// <summary>
        /// Sort in descending order.
        /// </summary>
        Descending
    }
}
