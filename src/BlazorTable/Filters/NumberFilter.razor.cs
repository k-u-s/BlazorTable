using BlazorTable.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;

namespace BlazorTable
{
    public partial class NumberFilter<TableItem> : IFilter<TableItem>
    {
        private Func<TableItem, IComparable?> _getter;
        
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Inject]
        Microsoft.Extensions.Localization.IStringLocalizer<BlazorTable.Localization.Localization> Localization { get; set; }

        private NumberCondition Condition { get; set; }

        private string FilterValue { get; set; }

        protected override void OnInitialized()
        {
            if (Column.Type.IsNumeric() && !Column.Type.GetNonNullableType().IsEnum)
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
                    FilterValue = filter.FilterValue.ToString();
                }
            }
        }

        public FilterEntry GetFilter()
        {
            if (Condition != NumberCondition.IsNull && Condition != NumberCondition.IsNotNull && string.IsNullOrEmpty(FilterValue))
            {
                return null;
            }

            var convertedValue = Convert.ChangeType(FilterValue, Column.Type.GetNonNullableType(), CultureInfo.InvariantCulture);
            return new NumberFilterEntry<TableItem>(_getter)
            {
                Condition = Condition,
                FilterValue = convertedValue as IComparable
            };
        }
    }

    public class NumberFilterEntry<TableItem> : InMemoryFilterEntry<TableItem>
    {
        private Func<TableItem, IComparable?> _getter;
        
        public NumberCondition Condition { get; set; }
        public IComparable FilterValue { get; set; }
        
        public NumberFilterEntry(Func<TableItem, IComparable?> getter)
        {
            _getter = getter;
        }

        public override IQueryable<TableItem> Filter(IQueryable<TableItem> query)
        {
            return Condition switch
            {
                NumberCondition.IsEqualTo => query.Where(el => FilterValue.CompareTo(_getter(el)) == 0),

                NumberCondition.IsNotEqualTo => query.Where(el =>  FilterValue.CompareTo(_getter(el)) != 0),

                NumberCondition.IsGreaterThanOrEqualTo => query.Where(el =>  FilterValue.CompareTo(_getter(el)) <= 0),

                NumberCondition.IsGreaterThan => query.Where(el =>  FilterValue.CompareTo(_getter(el)) < 0),

                NumberCondition.IsLessThanOrEqualTo => query.Where(el =>  FilterValue.CompareTo(_getter(el)) >= 0),

                NumberCondition.IsLessThan => query.Where(el =>  FilterValue.CompareTo(_getter(el)) > 0),

                NumberCondition.IsNull => query.Where(el => _getter(el) == null),

                NumberCondition.IsNotNull => query.Where(el => _getter(el) != null),

                _ => throw new ArgumentException(Condition + " is not defined!"),
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
