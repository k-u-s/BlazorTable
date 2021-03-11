using System.Collections.Generic;
using System.Linq;
using BlazorTable.Components.ServerSide;
using BlazorTable.Handlers;

namespace BlazorTable.Components.ClientSide
{
    public class FilterKnownHandlers<TableItem>
    {
        private readonly List<FilterHandle<TableItem>> _knownHandlers;
        
        public FilterKnownHandlers(IEnumerable<FilterHandle<TableItem>> knownHandlers)
        {
            _knownHandlers = knownHandlers.ToList();
        }

        public FilterHandle<TableItem> GetHandler(FilterEntry filter)
            => _knownHandlers.FirstOrDefault(el => el.CanHandle(filter));
    }
}