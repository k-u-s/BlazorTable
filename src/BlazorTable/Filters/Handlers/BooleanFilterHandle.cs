using System;
using System.Linq;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Handlers
{
    public class BooleanFilterHandle<TableItem> : FilterHandle<TableItem>
    {
        public override bool CanHandle(FilterEntry filter)
            => filter.Source == nameof(BooleanFilter<TableItem>);

        public override IQueryable<TableItem> Filter(FilterEntry filter, IColumn<TableItem> column, IQueryable<TableItem> query)
        {
            if (filter.Source != nameof(BooleanFilter<TableItem>))
                throw new ArgumentException(nameof(filter));
            
            if(!Enum.TryParse<BooleanCondition>(filter.Condition, out var condition))
                throw new ArgumentException(nameof(filter));

            var compiled = column.Field.Compile();
            Func<TableItem, bool?> getter = el => compiled(el) as bool?;
            return condition switch
            {
                BooleanCondition.True => query.Where(el => getter(el) == true),

                BooleanCondition.False => query.Where(el => getter(el) == false),

                BooleanCondition.IsNull => query.Where(el => getter(el) == null),

                BooleanCondition.IsNotNull => query.Where(el => getter(el) != null),

                _ => throw new ArgumentOutOfRangeException(nameof(condition)),
            };
        }
    }
}