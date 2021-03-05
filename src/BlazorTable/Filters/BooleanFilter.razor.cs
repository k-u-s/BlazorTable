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
    public partial class BooleanFilter<TableItem> : IFilter<TableItem>
    {
        private Func<TableItem, bool?> _getter;
        
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        private BooleanCondition Condition { get; set; }

        public List<Type> FilterTypes => new List<Type>()
        {
            typeof(bool)
        };

        protected override void OnInitialized()
        {
            if (FilterTypes.Contains(Column.Type.GetNonNullableType()))
            {
                Column.FilterControl = this;
                var getter = Column.Field.Compile();
                _getter = tableItem =>
                {
                    var value = getter(tableItem);
                    if (value is bool boolValue)
                        return boolValue;

                    return null;
                };
                
                if (Column.Filter is BooleanFilterEntry<TableItem> filter)
                {
                    Condition = filter.Condition;
                }
            }
        }

        private Func<TableItem, bool?> Getter(Func<TableItem, object> arg)
        {
            return tableItem =>
            {
                var value = arg(tableItem);
                if (value is bool boolValue)
                    return boolValue;

                return null;
            };
        }

        public FilterEntry GetFilter()
        {
            return new BooleanFilterEntry<TableItem>(_getter)
            {
                Condition = Condition
            };
        }
        
    }

    public class BooleanFilterEntry<TableItem> : InMemoryFilterEntry<TableItem>
    {
        private Func<TableItem, bool?> _getter;
        
        public BooleanCondition Condition { get; set; }

        public BooleanFilterEntry(Func<TableItem, bool?> getter)
        {
            _getter = getter;
        }
        
        public override IQueryable<TableItem> Filter(IQueryable<TableItem> query)
        {
            return Condition switch
            {
                BooleanCondition.True => query.Where(el => _getter(el) == true),

                BooleanCondition.False => query.Where(el => _getter(el) == false),

                BooleanCondition.IsNull => query.Where(el => _getter(el) == null),

                BooleanCondition.IsNotNull => query.Where(el => _getter(el) != null),

                _ => null,
            };
        }
    }

    public enum BooleanCondition
    {
        [LocalizedDescription("BooleanConditionTrue", typeof(Localization.Localization))]
        True,

        [LocalizedDescription("BooleanConditionFalse", typeof(Localization.Localization))]
        False,

        [LocalizedDescription("BooleanConditionIsNull", typeof(Localization.Localization))]
        IsNull,

        [LocalizedDescription("BooleanConditionIsNotNull", typeof(Localization.Localization))]
        IsNotNull
    }
}
