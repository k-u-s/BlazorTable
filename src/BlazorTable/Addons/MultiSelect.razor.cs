using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BlazorTable.Addons.Loaders;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorTable.Addons
{
    public partial class MultiSelect<TableItem, HintItem> : IFilter<TableItem>
    {
        private Func<TableItem, object?> _getter;

        [CascadingParameter(Name = "Column")] public IColumn<TableItem> Column { get; set; }
        [Parameter] public IReadOnlyCollection<HintItem> Hints { get; set; }
        [Parameter] public IHintsLoader<HintItem> HintsLoader { get; set; }

        [Parameter] public RenderFragment<HintItem> ItemDisplaySelector { get; set; } = (el) => builder => builder.AddContent(0, el.ToString());

        private List<HintItem> Values { get; set; } = new();

        private MultiSelectCondition Condition { get; set; }

        private List<HintItem> SelectedHints { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            EnsureParametersValid();
        }

        protected override async Task OnInitializedAsync()
        {
            Column.FilterControl = this;
            EnsureParametersValid();
            _getter = Column.Field.Compile();
            Values = await LoadHintsAsync(string.Empty).ConfigureAwait(false);

            if (Column.Filter is MultiSelectFilterEntry<TableItem, HintItem> filter)
            {
                Condition = filter.Condition;
                SelectedHints = filter.SelectedHints;
            }

            SelectedHints ??= Values.ToList();
        }


        public FilterEntry GetFilter()
        {
            return new MultiSelectFilterEntry<TableItem, HintItem>(_getter)
            {
                Condition = Condition,
                SelectedHints = SelectedHints
            };
        }

        public async Task RefreshFilterValues(string item)
        {
            Values = await LoadHintsAsync(item).ConfigureAwait(false);
            StateHasChanged();
        }

        private void EnsureParametersValid()
        {
            if (HintsLoader is not null)
                return;

            if (Hints is not null)
                HintsLoader = new InMemoryHintsLoader<HintItem>(Hints);
            else
                throw new ArgumentNullException(nameof(HintsLoader));
        }

        private async Task<List<HintItem>> LoadHintsAsync(string item)
        {
            var hintsResult = await HintsLoader.LoadHintsAsync(new()
            {
                Key = Column.Key,
                Hint = item,
                Skip = 0,
                Top = int.MaxValue
            }).ConfigureAwait(false);
            var hints = hintsResult.Records.ToList();
            return hints;
        }
    }

    public class MultiSelectFilterEntry<TableItem, HintItem> : InMemoryFilterEntry<TableItem>
    {
        private Func<TableItem, object?> _getter;

        public MultiSelectCondition Condition { get; set; }
        public List<HintItem> SelectedHints { get; set; }

        public MultiSelectFilterEntry(Func<TableItem, object?> getter)
        {
            _getter = getter;
        }

        public override IQueryable<TableItem> Filter(IQueryable<TableItem> query)
        {
            return Condition switch
            {
                MultiSelectCondition.Contains => query.Where(el => Contains(el)),

                MultiSelectCondition.IsNull => query.Where(el => _getter(el) == null),

                MultiSelectCondition.IsNotNull => query.Where(el => _getter(el) != null),

                _ => throw new ArgumentException(Condition + " is not defined!"),
            };
        }

        private bool Contains(TableItem item)
        {
            var itemValue = _getter(item);
            var value = itemValue switch
            {
                null => false,
                IFilterable<HintItem> filterable => SelectedHints.Any(hint => filterable.Contains(hint)),
                object val => SelectedHints.Any(hint => val.ToString().Contains(hint.ToString(), StringComparison.InvariantCultureIgnoreCase)),
            };
            return value;
        }
    }

    public enum MultiSelectCondition
    {
        [LocalizedDescription("StringConditionContains", typeof(Localization.Localization))]
        Contains,

        [LocalizedDescription("StringConditionIsNullOrEmpty", typeof(Localization.Localization))]
        IsNull,

        [LocalizedDescription("StringConditionIsNotNullOrEmpty", typeof(Localization.Localization))]
        IsNotNull
    }
}
