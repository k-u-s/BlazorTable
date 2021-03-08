using BlazorTable.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;

namespace BlazorTable
{
    public partial class StringFilter<TableItem> : IFilter<TableItem>
    {
        private Func<TableItem, string?> _getter;
        
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Inject]
        Microsoft.Extensions.Localization.IStringLocalizer<BlazorTable.Localization.Localization> Localization { get; set; }

        private StringCondition Condition { get; set; }

        private Func<string, string> _textField = el => el;
        
        private string FilterText { get; set; }

        public Type FilterType => typeof(string);
        
        protected override void OnInitialized()
        {
            if (Column.Type == typeof(string))
            {
                Column.FilterControl = this;
                
                var getter = Column.Field.Compile();
                _getter = tableItem =>
                {
                    var objectValue = getter(tableItem);
                    if (objectValue is string value)
                        return value;

                    return null;
                };
                if (Column.Filter != null)
                {
                    if (Column.Filter is StringFilterEntry<TableItem> filter)
                    {
                        Condition = filter.Condition;
                        FilterText = filter.FilterText;
                    }
                }
            }
        }

        public FilterEntry GetFilter()
        {
            FilterText = FilterText?.Trim();

            if (Condition != StringCondition.IsNullOrEmpty && Condition != StringCondition.IsNotNulOrEmpty && string.IsNullOrEmpty(FilterText))
            {
                return null;
            }

            return new StringFilterEntry<TableItem>(_getter)
            {
                Condition = Condition,
                FilterText = FilterText
            };
        }
    }

    public class StringFilterEntry<TableItem> : InMemoryFilterEntry<TableItem>
    {
        private Func<TableItem, string?> _getter;
        
        public StringCondition Condition { get; set; }
        public string FilterText { get; set; }

        public StringFilterEntry(Func<TableItem, string?> getter)
        {
            _getter = getter;
        }
        
        public override IQueryable<TableItem> Filter(IQueryable<TableItem> query)
        {
            return Condition switch
            {
                StringCondition.Contains => query.Where(el => _getter(el).Contains(FilterText)),

                StringCondition.DoesNotContain => query.Where(el => !_getter(el).Contains(FilterText)),

                StringCondition.StartsWith => query.Where(el => _getter(el).StartsWith(FilterText)),

                StringCondition.EndsWith => query.Where(el => _getter(el).EndsWith(FilterText)),

                StringCondition.IsEqualTo => query.Where(el => FilterText.Equals(_getter(el))),

                StringCondition.IsNotEqualTo => query.Where(el => !FilterText.Equals(_getter(el))),

                StringCondition.IsNullOrEmpty => query.Where(el => string.IsNullOrEmpty(_getter(el))),

                StringCondition.IsNotNulOrEmpty => query.Where(el => !string.IsNullOrEmpty(_getter(el))),
                
                _ => throw new ArgumentException(Condition + " is not defined!")
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
