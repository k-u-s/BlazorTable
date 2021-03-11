using System;
using System.Collections.Generic;
using System.Linq;
using BlazorTable.Addons.Handlers;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using Microsoft.AspNetCore.Components;

namespace BlazorTable.Addons
{
    public partial class CustomSelect<TableItem> : IFilter<TableItem>, ICustomSelect
    {
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        private List<KeyValuePair<string, object>> _items = new List<KeyValuePair<string, object>>();

        private CustomSelectCondition Condition { get; set; }

        internal object FilterValue { get; set; }

        protected override void OnInitialized()
        {
            Column.FilterControl = this;

            if (Column.Filter?.Source != nameof(CustomSelect<TableItem>))
                return;
                
            if(Enum.TryParse<CustomSelectCondition>(Column.Filter.Condition, out var condition))
            {
                Condition = condition;
                var parmName = nameof(FilterValue);
                if (Column.Filter.Parameters.ContainsKey(parmName))
                    FilterValue = Column.Filter.Parameters[parmName];
            }
        }

        public FilterEntry GetFilter()
        {
            return new ()
            {
                Key = Column.Key,
                Source = nameof(CustomSelect<TableItem>),
                Condition = Condition.ToString(),
                Parameters = new Dictionary<string, object>()
                {
                    {nameof(FilterValue), FilterValue}
                }
            };
        }

        public void AddSelect(string key, object value)
        {
            _items.Add(new KeyValuePair<string, object>(key, value));

            if (FilterValue == null)
            {
                FilterValue = _items.FirstOrDefault().Value;
            }

            StateHasChanged();
        }
    }
        
    public enum CustomSelectCondition
    {
        [LocalizedDescription("CustomSelectConditionIsEqualTo", typeof(Localization.Localization))]
        IsEqualTo,

        [LocalizedDescription("CustomSelectConditionIsNotEqualTo", typeof(Localization.Localization))]
        IsNotEqualTo,

        [LocalizedDescription("CustomSelectConditionIsNull", typeof(Localization.Localization))]
        IsNull,

        [LocalizedDescription("CustomSelectConditionIsNotNull", typeof(Localization.Localization))]
        IsNotNull
    }
}
