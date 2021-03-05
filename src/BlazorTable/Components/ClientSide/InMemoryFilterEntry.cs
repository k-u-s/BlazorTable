using System.Linq;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Components.ClientSide
{
    public abstract class InMemoryFilterEntry<TableItem> : FilterEntry
    {
        public abstract IQueryable<TableItem> Filter(IQueryable<TableItem> query);
    }
}