using BlazorTable.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Handlers;

namespace BlazorTable
{
    public partial class NumberFilter<TableItem> : IFilter<TableItem>
    {
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Inject]
        Microsoft.Extensions.Localization.IStringLocalizer<BlazorTable.Localization.Localization> Localization { get; set; }

        internal NumberCondition Condition { get; set; }

        internal string FilterNumber { get; set; }

        protected override void OnInitialized()
        {
            if (Column.Type.IsNumeric() && !Column.Type.GetNonNullableType().IsEnum)
            {
                Column.FilterControl = this;
                if (Column.Filter?.Source != nameof(NumberFilter<TableItem>))
                    return;
                
                if(Enum.TryParse<NumberCondition>(Column.Filter.Condition, out var condition))
                {
                    Condition = condition;
                    var parmName = nameof(FilterNumber);
                    if (Column.Filter.Parameters.ContainsKey(parmName))
                        FilterNumber = Column.Filter.Parameters[parmName]?.ToString();
                }
            }
        }

        public FilterEntry GetFilter()
        {
            var convertedValue = Convert.ChangeType(FilterNumber, Column.Type.GetNonNullableType(), CultureInfo.InvariantCulture);
            return new ()
            {
                Key = Column.Key,
                Source = nameof(NumberFilter<TableItem>),
                Condition = Condition.ToString(),
                Parameters = new Dictionary<string, object>()
                {
                    {nameof(FilterNumber), convertedValue}
                }
            };
        }
    }
    
    public enum NumberCondition
    {
        [LocalizedDescription("NumberConditionIsEqualTo", typeof(Localization.Localization))]
        IsEqualTo,

        [LocalizedDescription("NumberConditionIsnotEqualTo", typeof(Localization.Localization))]
        IsNotEqualTo,

        [LocalizedDescription("NumberConditionIsGreaterThanOrEqualTo", typeof(Localization.Localization))]
        IsGreaterThanOrEqualTo,

        [LocalizedDescription("NumberConditionIsGreaterThan", typeof(Localization.Localization))]
        IsGreaterThan,

        [LocalizedDescription("NumberConditionIsLessThanOrEqualTo", typeof(Localization.Localization))]
        IsLessThanOrEqualTo,

        [LocalizedDescription("NumberConditionIsLessThan", typeof(Localization.Localization))]
        IsLessThan,

        [LocalizedDescription("NumberConditionIsNull", typeof(Localization.Localization))]
        IsNull,

        [LocalizedDescription("NumberConditionIsNotNull", typeof(Localization.Localization))]
        IsNotNull
    }
}
