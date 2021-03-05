using BlazorTable.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using Microsoft.Extensions.Localization;

namespace BlazorTable
{
    public partial class EnumFilter<TableItem> : IFilter<TableItem>
    {
        private Func<TableItem, Enum?> _getter;
        
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Inject]
        IStringLocalizer<Localization.Localization> Localization { get; set; }

        private EnumCondition Condition { get; set; }

        private Enum FilterValue { get; set; }

        protected override void OnInitialized()
        {
            if (Column.Type.GetNonNullableType().IsEnum)
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

                if (Column.Filter is EnumFilterEntry<TableItem> filter)
                {
                    Condition = filter.Condition;
                    FilterValue = filter.FilterValue;
                }

                if (FilterValue == null)
                {
                    FilterValue = (Enum)Enum.GetValues(Column.Type.GetNonNullableType()).GetValue(0);
                }
            }
        }

        public FilterEntry GetFilter()
        {
            return new EnumFilterEntry<TableItem>(_getter)
            {
                Condition = Condition,
                FilterValue = FilterValue
            };
        }

        public class EnumFilterEntry<TableItem> : InMemoryFilterEntry<TableItem>
        {
            private Func<TableItem, Enum?> _getter;
        
            public EnumCondition Condition { get; set; }
            public Enum FilterValue { get; set; }
        
            public EnumFilterEntry(Func<TableItem, Enum?> getter)
            {
                _getter = getter;
            }

            public override IQueryable<TableItem> Filter(IQueryable<TableItem> query)
            {
                return Condition switch
                {
                    EnumCondition.IsEqualTo => query.Where(el => FilterValue.CompareTo(_getter(el)) == 0),

                    EnumCondition.IsNotEqualTo => query.Where(el =>  FilterValue.CompareTo(_getter(el)) != 0),

                    EnumCondition.IsNull => query.Where(el => _getter(el) == null),

                    EnumCondition.IsNotNull => query.Where(el => _getter(el) != null),

                    _ => throw new ArgumentException(Condition + " is not defined!"),
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
}
