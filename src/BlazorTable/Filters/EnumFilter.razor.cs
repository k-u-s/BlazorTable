using BlazorTable.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Handlers;
using Microsoft.Extensions.Localization;

namespace BlazorTable
{
    public partial class EnumFilter<TableItem> : IFilter<TableItem>
    {
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Inject]
        IStringLocalizer<Localization.Localization> Localization { get; set; }

        internal EnumCondition Condition { get; set; }

        internal Enum FilterValue { get; set; }

        protected override void OnInitialized()
        {
            if (Column.Type.GetNonNullableType().IsEnum)
            {
                Column.FilterControl = this;
                if (Column.Filter?.Source != nameof(EnumFilter<TableItem>))
                    return;
                
                if(Enum.TryParse<EnumCondition>(Column.Filter.Condition, out var condition))
                {
                    Condition = condition;
                    var parmName = nameof(FilterValue);
                    if (Column.Filter.Parameters.ContainsKey(parmName))
                        FilterValue = Column.Filter.Parameters[parmName] as Enum;
                }

                if (FilterValue == null)
                {
                    FilterValue = (Enum)Enum.GetValues(Column.Type.GetNonNullableType()).GetValue(0);
                }
            }
        }

        public FilterEntry GetFilter()
        {
            return new ()
            {
                Key = Column.Key,
                Source = nameof(EnumFilter<TableItem>),
                Condition = Condition.ToString(),
                Parameters = new Dictionary<string, object>()
                {
                    {nameof(FilterValue), FilterValue}
                }
            };
        }
    }

    public enum EnumCondition
    {
        [LocalizedDescription("EnumConditionIsEqualTo", typeof(Localization.Localization))]
        IsEqualTo,

        [LocalizedDescription("EnumConditionIsNotEqualTo", typeof(Localization.Localization))]
        IsNotEqualTo,

        [LocalizedDescription("EnumConditionIsNull", typeof(Localization.Localization))]
        IsNull,

        [LocalizedDescription("EnumConditionIsNotNull", typeof(Localization.Localization))]
        IsNotNull
    }
}
