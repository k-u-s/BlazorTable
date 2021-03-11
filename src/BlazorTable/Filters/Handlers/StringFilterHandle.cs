using System;
using System.Linq;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Handlers
{
    public class StringFilterHandle<TableItem> : FilterHandle<TableItem>
    {
        public override bool CanHandle(FilterEntry filter)
            => filter.Source == nameof(StringFilter<TableItem>);

        public override IQueryable<TableItem> Filter(FilterEntry filter, IColumn<TableItem> column, IQueryable<TableItem> query)
        {
            if (filter.Source != nameof(StringFilter<TableItem>))
                throw new ArgumentException(nameof(filter));
            
            if(!Enum.TryParse<StringCondition>(filter.Condition, out var condition))
                throw new ArgumentException(nameof(filter.Condition));

            var paramName = nameof(StringFilter<TableItem>.FilterText);
            if(filter.Parameters?.ContainsKey(paramName) != true)
                throw new ArgumentException(paramName);

            var filterText = filter.Parameters[paramName] as string 
                              ?? throw new ArgumentException(paramName);
            var compiled = column.Field.Compile();
            Func<TableItem, string> getter = el => (compiled(el) as string) ?? string.Empty;
            return condition switch
            {
                StringCondition.Contains => query.Where(el => getter(el).Contains(filterText)),

                StringCondition.DoesNotContain => query.Where(el => !getter(el).Contains(filterText)),

                StringCondition.StartsWith => query.Where(el => getter(el).StartsWith(filterText)),

                StringCondition.EndsWith => query.Where(el => getter(el).EndsWith(filterText)),

                StringCondition.IsEqualTo => query.Where(el => filterText.Equals(getter(el))),

                StringCondition.IsNotEqualTo => query.Where(el => !filterText.Equals(getter(el))),

                StringCondition.IsNullOrEmpty => query.Where(el => string.IsNullOrEmpty(getter(el))),

                StringCondition.IsNotNulOrEmpty => query.Where(el => !string.IsNullOrEmpty(getter(el))),

                _ => throw new ArgumentOutOfRangeException(nameof(condition)),
            };
        }
    }
}