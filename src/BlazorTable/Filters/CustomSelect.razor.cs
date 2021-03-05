using BlazorTable.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;

namespace BlazorTable
{
    public partial class CustomSelect<TableItem> : IFilter<TableItem>, ICustomSelect
    {
        private Func<TableItem, object?> _getter;
        
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        private List<KeyValuePair<string, object>> Items = new List<KeyValuePair<string, object>>();

        private CustomSelectCondition Condition { get; set; }

        private object FilterValue { get; set; }

        protected override void OnInitialized()
        {
            Column.FilterControl = this;
            var getter = Column.Field.Compile();
            _getter = tableItem =>
            {
                var objectValue = getter(tableItem);
                if (objectValue is Enum value)
                    return value;

                return null;
            };

            if (Column.Filter is CustomFilterEntry<TableItem> filter)
            {
                Condition = filter.Condition;
                FilterValue = filter.FilterValue;
            }
        }

        public FilterEntry GetFilter()
        {
            return new CustomFilterEntry<TableItem>(_getter)
            {
                Condition = Condition,
                FilterValue = FilterValue
            };
        }

        public void AddSelect(string key, object value)
        {
            Items.Add(new KeyValuePair<string, object>(key, value));

            if (FilterValue == null)
            {
                FilterValue = Items.FirstOrDefault().Value;
            }

            StateHasChanged();
        }


        public class CustomFilterEntry<TableItem> : InMemoryFilterEntry<TableItem>
        {
            private Func<TableItem, object?> _getter;
        
            public CustomSelectCondition Condition { get; set; }
            public object FilterValue { get; set; }
        
            public CustomFilterEntry(Func<TableItem, object?> getter)
            {
                _getter = getter;
            }

            public override IQueryable<TableItem> Filter(IQueryable<TableItem> query)
            {
                return Condition switch
                {
                    CustomSelectCondition.IsEqualTo => query.Where(el => FilterValue.Equals(_getter(el))),

                    CustomSelectCondition.IsNotEqualTo => query.Where(el => !FilterValue.Equals(_getter(el))),

                    CustomSelectCondition.IsNull => query.Where(el => _getter(el) == null),

                    CustomSelectCondition.IsNotNull => query.Where(el => _getter(el) != null),

                    _ => throw new ArgumentException(Condition + " is not defined!"),
                };
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
}
