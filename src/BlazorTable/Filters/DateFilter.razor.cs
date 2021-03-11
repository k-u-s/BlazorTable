using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Handlers;

namespace BlazorTable
{
    public partial class DateFilter<TableItem> : IFilter<TableItem>
    {
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        private NumberCondition Condition { get; set; }

        private DateTime FilterDate { get; set; } = DateTime.Now;

        protected override void OnInitialized()
        {
            if (Column.Type.GetNonNullableType() == typeof(DateTime))
            {
                Column.FilterControl = this;
                if (Column.Filter?.Source != nameof(DateFilter<TableItem>))
                    return;

                if(Enum.TryParse<NumberCondition>(Column.Filter.Condition, out var condition))
                {
                    Condition = condition;
                    var parmName = nameof(FilterDate);
                    if (Column.Filter.Parameters.ContainsKey(parmName))
                        FilterDate = Column.Filter.Parameters[parmName] as DateTime? ?? DateTime.Now;
                }
            }
        }

        public FilterEntry GetFilter()
        {
            return new ()
            {
                Key = Column.Key,
                Source = nameof(DateFilter<TableItem>),
                Condition = Condition.ToString(),
                Parameters = new Dictionary<string, object>()
                {
                    {nameof(NumberFilter<TableItem>.FilterNumber), FilterDate}
                }
            };
        }
    }
}