using System;
using System.Globalization;
using System.Linq;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Handlers
{
    public class NumberFilterHandle<TableItem> : FilterHandle<TableItem>
    {
        public override bool CanHandle(FilterEntry filter)
            => filter.Source == nameof(NumberFilter<TableItem>)
                || filter.Source == nameof(DateFilter<TableItem>);
        
        public override IQueryable<TableItem> Filter(FilterEntry filter, IColumn<TableItem> column, IQueryable<TableItem> query)
        {
            if (filter.Source != nameof(NumberFilter<TableItem>))
                throw new ArgumentException(nameof(filter));
            
            if(!Enum.TryParse<NumberCondition>(filter.Condition, out var condition))
                throw new ArgumentException(nameof(filter.Condition));

            var paramName = nameof(NumberFilter<TableItem>.FilterNumber);
            if(filter.Parameters?.ContainsKey(paramName) != true)
                throw new ArgumentException(nameof(filter.Parameters));
            
            var filterValue = filter.Parameters[paramName]
                              ?? throw new ArgumentException(paramName);
            
            var convertedValue = Convert.ChangeType(filterValue, column.Type.GetNonNullableType(), CultureInfo.InvariantCulture) 
                as IComparable;
            var compiled = column.Field.Compile();
            Func<TableItem, IComparable?> getter = el => compiled(el) as IComparable;
            return condition switch
            {
                NumberCondition.IsEqualTo => query.Where(el => convertedValue.CompareTo(getter(el)) == 0),

                NumberCondition.IsNotEqualTo => query.Where(el =>  convertedValue.CompareTo(getter(el)) != 0),

                NumberCondition.IsGreaterThanOrEqualTo => query.Where(el =>  convertedValue.CompareTo(getter(el)) <= 0),

                NumberCondition.IsGreaterThan => query.Where(el =>  convertedValue.CompareTo(getter(el)) < 0),

                NumberCondition.IsLessThanOrEqualTo => query.Where(el =>  convertedValue.CompareTo(getter(el)) >= 0),

                NumberCondition.IsLessThan => query.Where(el =>  convertedValue.CompareTo(getter(el)) > 0),

                NumberCondition.IsNull => query.Where(el => getter(el) == null),

                NumberCondition.IsNotNull => query.Where(el => getter(el) != null),

                _ => throw new ArgumentOutOfRangeException(nameof(condition)),
            };
        }
    }
}