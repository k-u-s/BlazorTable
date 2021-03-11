using System;
using System.Linq;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Addons.Handlers
{
    public class CustomSelectHandler<TableItem> : FilterHandle<TableItem>
    {
        public override bool CanHandle(FilterEntry filter)
            => filter.Source == nameof(CustomSelect<TableItem>);

        public override IQueryable<TableItem> Filter(FilterEntry filter, IColumn<TableItem> column,
            IQueryable<TableItem> query)
        {
            if (filter.Source != nameof(CustomSelect<TableItem>))
                throw new ArgumentException(nameof(filter));

            if (!Enum.TryParse<CustomSelectCondition>(filter.Condition, out var condition))
                throw new ArgumentException(nameof(filter.Condition));

            var paramName = nameof(CustomSelect<TableItem>.FilterValue);
            if (filter.Parameters?.ContainsKey(paramName) != true)
                throw new ArgumentException(nameof(filter.Parameters));

            var filterValue = filter.Parameters[paramName]
                              ?? throw new ArgumentException(paramName);
            var compiled = column.Field.Compile();
            Func<TableItem, object?> getter = el => compiled(el);
            return condition switch
            {
                CustomSelectCondition.IsEqualTo => query.Where(el => filterValue.Equals(getter(el))),

                CustomSelectCondition.IsNotEqualTo => query.Where(el => !filterValue.Equals(getter(el))),

                CustomSelectCondition.IsNull => query.Where(el => getter(el) == null),

                CustomSelectCondition.IsNotNull => query.Where(el => getter(el) != null),

                _ => throw new ArgumentOutOfRangeException(nameof(condition)),
            };
        }
    }
}