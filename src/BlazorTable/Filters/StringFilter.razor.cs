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

        private Lazy<List<string>> _allFilterValues = new (() => new List<string>());

        private List<string> AllFilterValues => _allFilterValues.Value;

        private List<string> SelectedFilterValues { get; set; } = new ();
        
        public Type FilterType => typeof(string);
        
        protected override void OnInitialized()
        {
            if (Column.Type == typeof(string))
            {
                Column.FilterControl = this;
                _allFilterValues = new (() =>
                {
                    var dataLoader = Column.Table.DataLoader;
                    var result = dataLoader.GetHints(new SearchHints
                    {
                        Key = Column.Key,
                    }).ConfigureAwait(false).GetAwaiter().GetResult();;
                    return result.Records.ToList();
                });
                
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
                        SelectedFilterValues = filter.SelectedFilterValues;
                        FilterText = filter.FilterText;
                    }
                    
                }
            }
        }

        public FilterEntry GetFilter()
        {
            FilterText = FilterText?.Trim();

            if ((Condition != StringCondition.IsNullOrEmpty && Condition != StringCondition.IsNotNulOrEmpty && string.IsNullOrEmpty(FilterText))
                && !(Condition == StringCondition.IsOneOf && SelectedFilterValues.Count > 0))
            {
                return null;
            }

            return new StringFilterEntry<TableItem>(_getter)
            {
                Condition = Condition,
                FilterText = FilterText,
                SelectedFilterValues = SelectedFilterValues
            };
        }
    }

    public class StringFilterEntry<TableItem> : InMemoryFilterEntry<TableItem>
    {
        private Func<TableItem, string?> _getter;
        
        public StringCondition Condition { get; set; }
        public string FilterText { get; set; }
        public List<string> SelectedFilterValues { get; set; }

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

                StringCondition.IsOneOf => query.Where(el => SelectedFilterValues.Contains(_getter(el))),
                
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

        [LocalizedDescription("StringConditionIsOneOf", typeof(Localization.Localization))]
        IsOneOf
    }
}
