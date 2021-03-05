using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;
using System.Linq.Expressions;
using BlazorTable.Components.ServerSide;

namespace BlazorTable
{
    public partial class DateFilter<TableItem> : IFilter<TableItem>
    {
        private Func<TableItem, IComparable?> _getter;
        
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        private NumberCondition Condition { get; set; }

        private DateTime FilterValue { get; set; } = DateTime.Now;

        protected override void OnInitialized()
        {
            if (Column.Type.GetNonNullableType() == typeof(DateTime))
            {
                Column.FilterControl = this;
                var getter = Column.Field.Compile();
                _getter = tableItem =>
                {
                    var objectValue = getter(tableItem);
                    if (objectValue is IComparable value)
                        return value;

                    return null;
                };

                if (Column.Filter is NumberFilterEntry<TableItem> filter)
                {
                    Condition = filter.Condition;
                    FilterValue = filter.FilterValue is DateTime time ? time : DateTime.Now;
                }
            }
        }

        public FilterEntry GetFilter()
        {
            return new NumberFilterEntry<TableItem>(_getter)
            {
                Condition = Condition,
                FilterValue = FilterValue
            };
        }
    }
}