using System;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components.ServerSide;

namespace BlazorTable
{
    /// <summary>
    /// Filter Component Interface
    /// </summary>
    /// <typeparam name="TableItem"></typeparam>
    public interface IFilter<TableItem>
    {
        /// <summary>
        /// Get Filter Data
        /// </summary>
        /// <returns></returns>
        FilterEntry GetFilter();
    }
}
