using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Interfaces;

namespace BlazorTable.Addons.Handlers
{
    public class MultiSelectHandler<TableItem> : FilterHandle<TableItem>
    {
        public override bool CanHandle(FilterEntry filter)
            => filter.Source == nameof(MultiSelect<TableItem>);

        public override IQueryable<TableItem> Filter(FilterEntry filter, IColumn<TableItem> column,
            IQueryable<TableItem> query)
        {
            if (filter.Source != nameof(MultiSelect<TableItem>))
                throw new ArgumentException(nameof(filter));

            if (!Enum.TryParse<MultiSelectCondition>(filter.Condition, out var condition))
                throw new ArgumentException(nameof(filter.Condition));

            var paramName = nameof(MultiSelect<TableItem>.SelectedHints);
            if (filter.Parameters.Count == 0)
                throw new ArgumentException(nameof(filter.Parameters));

            var selectedHints = filter.Parameters.Keys;
            var compiled = column.Field.Compile();
            Func<TableItem, object?> getter = el => compiled(el);
            var comparer = new HintsComparer<TableItem>(getter, selectedHints.OfType<object>().ToList());
            return condition switch
            {
                MultiSelectCondition.Contains => query.Where(el => comparer.Contains(el)),

                MultiSelectCondition.IsNull => query.Where(el => getter(el) == null),

                MultiSelectCondition.IsNotNull => query.Where(el => getter(el) != null),

                _ => throw new ArgumentOutOfRangeException(nameof(condition)),
            };
        }

        private class HintsComparer<TableItem>
        {
            private Func<TableItem, object?> _getter;
            private List<object> _selectedHints;
            
            public HintsComparer(Func<TableItem, object?> getter, 
                List<object> selectedHints)
            {
                _getter = getter;
                _selectedHints = selectedHints;
            }
            
            public bool Contains(TableItem item)
            {
                var itemValue = _getter(item);
                var value = itemValue switch
                {
                    null => false,
                    IFilterable<object> filterable => _selectedHints.Any(hint => filterable.Contains(hint)),
                    object val => _selectedHints.Any(hint => val.ToString().Contains(hint.ToString(), StringComparison.InvariantCultureIgnoreCase)),
                };
                return value;
            }
            
        }
    }
}