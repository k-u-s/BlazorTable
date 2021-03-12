﻿using System.Threading.Tasks;
using BlazorTable.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BlazorTable.Components
{
    public partial class FilterManager<TableItem>
    {
        [CascadingParameter(Name = "Column")]
        public IColumn<TableItem> Column { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Inject]
        public ILogger<FilterManager<TableItem>> Logger { get; set; }

        [Inject]
        IStringLocalizer<Localization.Localization> Localization { get; set; }

        private async Task ApplyFilterAsync()

        {
            Column.ToggleFilter();

            if (Column.FilterControl != null)
            {
                Column.Filter = Column.FilterControl.GetFilter();
                
                Column.Table.OnFilterChanged?.Invoke(new FilterChanged<TableItem>
                {
                    Column = Column
                });
                
                await Column.Table.UpdateAsync().ConfigureAwait(false);
                await Column.Table.FirstPageAsync().ConfigureAwait(false);
            }
            else
            {
                Logger.LogInformation("Filter is null");
            }
        }

        private async Task ClearFilterAsync()
        {
            Column.ToggleFilter();

            if (Column.Filter != null)
            {
                Column.Filter = null;
                
                Column.Table.OnFilterChanged?.Invoke(new FilterChanged<TableItem>
                {
                    Column = Column
                });
                
                await Column.Table.UpdateAsync().ConfigureAwait(false);
            }
        }
    }
}
