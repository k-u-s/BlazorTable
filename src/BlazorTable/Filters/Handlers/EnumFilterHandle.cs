using System;
using System.Linq;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Handlers
{
    public class EnumFilterHandle<TableItem> : FilterHandle<TableItem>
    {
        public override bool CanHandle(FilterEntry filter)
            => filter.Source == nameof(EnumFilter<TableItem>);

        public override IQueryable<TableItem> Filter(FilterEntry filter, IColumn<TableItem> column, IQueryable<TableItem> query)
        {
            if (filter.Source != nameof(EnumFilter<TableItem>))
                throw new ArgumentException(nameof(filter));
            
            if(!Enum.TryParse<EnumCondition>(filter.Condition, out var condition))
                throw new ArgumentException(nameof(filter.Condition));

            var paramName = nameof(EnumFilter<TableItem>.FilterValue);
            if(filter.Parameters?.ContainsKey(paramName) != true)
                throw new ArgumentException(nameof(filter.Parameters));

            var filterValue = filter.Parameters[paramName] as Enum 
                              ?? throw new ArgumentException(paramName);
            var compiled = column.Field.Compile();
            Func<TableItem, bool?> getter = el => compiled(el) as bool?;
            return condition switch
            {
                EnumCondition.IsEqualTo => query.Where(el => filterValue.CompareTo(getter(el)) == 0),

                EnumCondition.IsNotEqualTo => query.Where(el =>  filterValue.CompareTo(getter(el)) != 0),

                EnumCondition.IsNull => query.Where(el => getter(el) == null),

                EnumCondition.IsNotNull => query.Where(el => getter(el) != null),

                _ => throw new ArgumentOutOfRangeException(nameof(condition)),
            };
        }
    }
}