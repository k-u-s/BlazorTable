using BlazorTable.Localization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Handlers;

namespace BlazorTable
{
    public partial class BooleanFilter<TableItem> : IFilter<TableItem>
    {
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
                if (Column.Filter?.Source != nameof(BooleanFilter<TableItem>))
                    return;

                if(Enum.TryParse<BooleanCondition>(Column.Filter.Condition, out var condition))
                {
                    Condition = condition;
                }
            }
        }

        public FilterEntry GetFilter()
        {
            return new ()
            {
                Key = Column.Key,
                Source = nameof(BooleanFilter<TableItem>),
                Condition = Condition.ToString()
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
