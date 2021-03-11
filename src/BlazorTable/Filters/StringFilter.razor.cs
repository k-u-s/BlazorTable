using BlazorTable.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Handlers;

namespace BlazorTable
{
    public partial class StringFilter<TableItem> : IFilter<TableItem>
    {
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Inject]
        Microsoft.Extensions.Localization.IStringLocalizer<BlazorTable.Localization.Localization> Localization { get; set; }


        private Func<string, string> _textField = el => el;
        
        internal StringCondition Condition { get; set; }
        
        internal string FilterText { get; set; }

        public Type FilterType => typeof(string);
        
        protected override void OnInitialized()
        {
            if (Column.Type == typeof(string))
            {
                Column.FilterControl = this;
                if (Column.Filter?.Source != nameof(StringFilter<TableItem>))
                    return;
                
                if(Enum.TryParse<StringCondition>(Column.Filter.Condition, out var condition))
                {
                    Condition = condition;
                    var parmName = nameof(FilterText);
                    if (Column.Filter.Parameters.ContainsKey(parmName))
                        FilterText = Column.Filter.Parameters[parmName]?.ToString();
                }
            }
        }

        public FilterEntry GetFilter()
        {
            return new ()
            {
                Key = Column.Key,
                Source = nameof(NumberFilter<TableItem>),
                Condition = Condition.ToString(),
                Parameters = new Dictionary<string, object>()
                {
                    {nameof(FilterText), FilterText}
                }
            };
        }
    }

    public enum StringCondition
    {
        [LocalizedDescription("StringConditionContains", typeof(Localization.Localization))]
        Contains,

        [LocalizedDescription("StringConditionDoesNotContain", typeof(Localization.Localization))]
        DoesNotContain,

        [LocalizedDescription("StringConditionStartsWith", typeof(Localization.Localization))]
        StartsWith,

        [LocalizedDescription("StringConditionEndsWith", typeof(Localization.Localization))]
        EndsWith,

        [LocalizedDescription("StringConditionIsEqualTo", typeof(Localization.Localization))]
        IsEqualTo,

        [LocalizedDescription("StringConditionIsNotEqualTo", typeof(Localization.Localization))]
        IsNotEqualTo,

        [LocalizedDescription("StringConditionIsNullOrEmpty", typeof(Localization.Localization))]
        IsNullOrEmpty,

        [LocalizedDescription("StringConditionIsNotNullOrEmpty", typeof(Localization.Localization))]
        IsNotNulOrEmpty,
    }
}
