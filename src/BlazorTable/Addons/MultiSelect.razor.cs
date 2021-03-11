using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BlazorTable.Addons.Handlers;
using BlazorTable.Addons.Loaders;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorTable.Addons
{
    public partial class MultiSelect<TableItem> : IFilter<TableItem>
    {
        [CascadingParameter(Name = "Column")] public IColumn<TableItem> Column { get; set; }
        [Parameter] public IHintsLoader<object> HintsLoader { get; set; }

        [Parameter] public RenderFragment<object> ItemDisplaySelector { get; set; } = (el) => builder => builder.AddContent(0, el.ToString());

        private IReadOnlyCollection<object> Hints { get; set; }
        private List<object> Values { get; set; } = new();

        internal MultiSelectCondition Condition { get; set; }

        internal List<object> SelectedHints { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            EnsureParametersValid();
        }

        protected override async Task OnInitializedAsync()
        {
            Column.FilterControl = this;
            EnsureParametersValid();
            Values = await LoadHintsAsync(string.Empty).ConfigureAwait(false);
            try
            {
                if (Column.Filter?.Source != nameof(MultiSelect<TableItem>))
                    return;

                if (Enum.TryParse<MultiSelectCondition>(Column.Filter.Condition, out var condition))
                {
                    Condition = condition;
                    var parmName = nameof(SelectedHints);
                    if (Column.Filter.Parameters.ContainsKey(parmName))
                        SelectedHints = Column.Filter.Parameters[parmName] as List<object>;
                }
            }
            finally
            {
                SelectedHints ??= Values.ToList();
            }
        }


        public FilterEntry GetFilter()
        {
            return new ()
            {
                Key = Column.Key,
                Source = nameof(MultiSelect<TableItem>),
                Condition = Condition.ToString(),
                Parameters = SelectedHints.ToDictionary(el => el.ToString(), el => el)
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

            var getter = Column.Field.Compile();
            Hints = Column.Table.Items.Select(el => getter(el))
                .ToList();
            HintsLoader = new InMemoryHintsLoader(Hints);
        }

        private async Task<List<object>> LoadHintsAsync(string item)
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
