using System.Linq;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Components.ClientSide
{
    public abstract class FilterHandle<TableItem>
    {
        public abstract bool CanHandle(FilterEntry filter);
        
        public abstract IQueryable<TableItem> Filter(FilterEntry filter, IColumn<TableItem> column, IQueryable<TableItem> query);
    }
}