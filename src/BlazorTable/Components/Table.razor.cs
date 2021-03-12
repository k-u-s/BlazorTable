using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BlazorTable.Components.ClientSide;
using BlazorTable.Components.ServerSide;
using BlazorTable.Events;
using BlazorTable.Interfaces;
using LinqKit;
using Microsoft.AspNetCore.Components;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace BlazorTable.Components
{
    public partial class Table<TableItem> : ITable<TableItem>
    {
        private const int DEFAULT_PAGE_SIZE = 10;

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> UnknownParameters { get; set; }

        /// <summary>
        /// Table CSS Class (Defaults to Bootstrap 4)
        /// </summary>
        [Parameter]
        public string TableClass { get; set; } = "table table-striped table-bordered table-hover table-sm";

        /// <summary>
        /// Table Head Class (Defaults to Bootstrap 4)
        /// </summary>
        [Parameter]
        public string TableHeadClass { get; set; } = "thead-light text-dark";

        /// <summary>
        /// Table Body Class
        /// </summary>
        [Parameter]
        public string TableBodyClass { get; set; } = "";

        /// <summary>
        /// Table Footer Class
        /// </summary>
        [Parameter]
        public string TableFooterClass { get; set; } = "text-white bg-secondary";

        /// <summary>
        /// Expression to set Row Class
        /// </summary>
        [Parameter]
        public Func<TableItem, string> TableRowClass { get; set; }

        /// <summary>
        /// Page Size, defaults to 15
        /// </summary>
        [Parameter]
        public int PageSize { get; set; } = DEFAULT_PAGE_SIZE;

        /// <summary>
        /// Allow Columns to be reordered
        /// </summary>
        [Parameter]
        public bool ColumnReorder { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        /// <summary>
        /// IQueryable data source to display in the table
        /// </summary>
        [Parameter]
        public IQueryable<TableItem> ItemsQueryable { get; set; }

        /// <summary>
        /// Collection to display in the table
        /// </summary>
        [Parameter]
        public IEnumerable<TableItem> Items { get; set; }

        /// <summary>
        /// Service to use to query data server side
        /// </summary>
        [Parameter]
        public IDataLoader<TableItem> DataLoader { get; set; }

        /// <summary>
        /// Search all columns for the specified string, supports spaces as a delimiter
        /// </summary>
        [Parameter]
        public string GlobalSearch { get; set; }

        /// <summary>
        /// Callback triggered after sort order changes
        /// </summary>
        [Parameter]
        public Action<SortChanged<TableItem>> OnSortChanged { get; set; }
        
        /// <summary>
        /// Callback triggered after filter changes
        /// </summary>
        [Parameter]
        public Action<FilterChanged<TableItem>> OnFilterChanged { get; set; }
        
        [Inject]
        private ILogger<ITable<TableItem>> Logger { get; set; }

        [Inject]
        IStringLocalizer<Localization.Localization> Localization { get; set; }

        [Inject]
        internal FilterKnownHandlers<TableItem> FiltersKnown { get; set; }

        /// <summary>
        /// Ref to visibility menu icon for popover display
        /// </summary>
        private ElementReference VisibilityMenuIconRef { get; set; }

        /// <summary>
        /// True if visibility menu is open otherwise false
        /// </summary>
        private bool VisibilityMenuOpen { get; set; }

        /// <summary>
        /// Collection of filtered items
        /// </summary>
        public IEnumerable<TableItem> FilteredItems { get; private set; }

        /// <summary>
        /// List of All Available Columns
        /// </summary>
        public List<IColumn<TableItem>> Columns { get; } = new List<IColumn<TableItem>>();

        /// <summary>
        /// Current Page Number
        /// </summary>
        public int PageNumber { get; private set; }

        /// <summary>
        /// Total Count of Items
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Is Table in Edit mode
        /// </summary>
        public bool IsEditMode { get; private set; }

        /// <summary>
        /// Total Pages
        /// </summary>
        public int TotalPages => PageSize <= 0 ? 1 : (TotalCount + PageSize - 1) / PageSize;

        /// <summary>
        /// Custom Rows
        /// </summary>
        private List<CustomRow<TableItem>> CustomRows { get; set; } = new List<CustomRow<TableItem>>();

        protected override async Task OnParametersSetAsync()
        {
            DataLoader ??= new InMemoryDataLoader<TableItem>(this);

            if (Columns is not null)
            {
                
            }
            
            await UpdateAsync().ConfigureAwait(false);
        }

        private Dictionary<int, bool> detailsViewOpen = new Dictionary<int, bool>();

        /// <summary>
        /// Open/Close detail view in specified row.
        /// </summary>
        /// <param name="row">number of row to toggle detail view</param>
        /// <param name="open">true for openening detail view, false for closing detail view</param>
        public void ToggleDetailView(int row, bool open)
        {
            if (!detailsViewOpen.ContainsKey(row))
                throw new KeyNotFoundException("Specified row could not be found in the currently rendered part of the table.");

            detailsViewOpen[row] = open;
        }

        /// <summary>
        /// Open/Close all detail views.
        /// </summary>
        /// <param name="open">true for openening detail view, false for closing detail view</param>
        public void ToggleAllDetailsView(bool open)
        {
            int[] rows = new int[detailsViewOpen.Keys.Count];
            detailsViewOpen.Keys.CopyTo(rows, 0);
            foreach (int row in rows)
            {
                detailsViewOpen[row] = open;
            }
        }

        /// <summary>
        /// Gets Data and redraws the Table
        /// </summary>
        public async Task UpdateAsync()
        {
            var result = await LoadDataAsync().ConfigureAwait(false);
            FilteredItems = result.Records;
            Refresh();
        }

        private async Task<PaginationResult<TableItem>> LoadDataAsync()
        {
            var sortColumn = Columns.FirstOrDefault(x => x.SortColumn);
            var sortKey = sortColumn?.Key;
            var sortDirection = sortColumn?.SortDescending switch
            {
                true => SortDirection.Descending,
                false => SortDirection.Ascending,
                null => SortDirection.UnSet
            };
            var filters = Columns
                    .Where(el => el.Filter is not null)
                    .ToDictionary(el => el.Key, el => el.Filter)
                ;
            var result = await DataLoader.LoadDataAsync(new FilterData
            {
                Top = PageSize,
                Skip = PageNumber * PageSize,
                GlobalSearchQuery = GlobalSearch,
                FilterEntries = filters,
                OrderBy = sortKey,
                OrderDirection = sortDirection
            }).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Adds a Column to the Table
        /// </summary>
        /// <param name="column"></param>
        public void AddColumn(IColumn<TableItem> column)
        {
            column.Table = this;

            if (column.Type == null)
            {
                column.Type = column.Field?.GetPropertyMemberInfo().GetMemberUnderlyingType();
            }

            column.DisplayIndex = column.DisplayIndex == 0 ? Columns.Count :  column.DisplayIndex;
            Columns.Add(column);
            Refresh();
        }

        /// <summary>
        /// Removes a Column from the Table
        /// </summary>
        /// <param name="column"></param>
        public void RemoveColumn(IColumn<TableItem> column)
        {
            Columns.Remove(column);
            Refresh();
        }

        /// <summary>
        /// Go to First Page
        /// </summary>
        public async Task FirstPageAsync()
        {
            if (PageNumber != 0)
            {
                PageNumber = 0;
                detailsViewOpen.Clear();
                await UpdateAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Go to Next Page
        /// </summary>
        public async Task NextPageAsync()
        {
            if (PageNumber + 1 < TotalPages)
            {
                PageNumber++;
                detailsViewOpen.Clear();
                await UpdateAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Go to Previous Page
        /// </summary>
        public async Task PreviousPageAsync()
        {
            if (PageNumber > 0)
            {
                PageNumber--;
                detailsViewOpen.Clear();
                await UpdateAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Go to Last Page
        /// </summary>
        public async Task LastPageAsync()
        {
            PageNumber = TotalPages - 1;
            detailsViewOpen.Clear();
            await UpdateAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Redraws the Table using EditTemplate instead of Template
        /// </summary>
        public void ToggleEditMode()
        {
            IsEditMode = !IsEditMode;
            StateHasChanged();
        }

        /// <summary>
        /// Redraws Table without Getting Data
        /// </summary>
        public void Refresh()
        {
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Save currently dragged column
        /// </summary>
        private IColumn<TableItem> DragSource;

        /// <summary>
        /// Handles the Column Reorder Drag Start and set DragSource
        /// </summary>
        /// <param name="column"></param>
        private void HandleDragStart(IColumn<TableItem> column)
        {
            DragSource = column;
        }

        /// <summary>
        /// Handles Drag Drop and inserts DragSource column before itself
        /// </summary>
        /// <param name="column"></param>
        private void HandleDrop(IColumn<TableItem> column)
        {
            if (DragSource != null)
            {
                var targetColumn = Columns.FirstOrDefault(a => a == column);
                if (targetColumn is null)
                    return;
                
                var targetIndex = targetColumn.DisplayIndex;
                foreach (var col in Columns.Where(el => el.DisplayIndex >= targetIndex))
                    col.DisplayIndex++;
                
                DragSource.DisplayIndex = targetIndex;
                DragSource = null;

                StateHasChanged();
            }
        }

        /// <summary>
        /// Return row class for item if expression is specified
        /// </summary>
        /// <param name="item">TableItem to return for</param>
        /// <returns></returns>
        private string RowClass(TableItem item)
        {
            return TableRowClass?.Invoke(item);
        }

        /// <summary>
        /// Set the template to use for empty data
        /// </summary>
        /// <param name="emptyDataTemplate"></param>
        public void SetEmptyDataTemplate(EmptyDataTemplate emptyDataTemplate)
        {
            _emptyDataTemplate = emptyDataTemplate?.ChildContent;
        }

        private RenderFragment _emptyDataTemplate;

        /// <summary>
        /// Set the template to use for loading data
        /// </summary>
        /// <param name="loadingDataTemplate"></param>
        public void SetLoadingDataTemplate(LoadingDataTemplate loadingDataTemplate)
        {
            _loadingDataTemplate = loadingDataTemplate?.ChildContent;
        }

        private RenderFragment _loadingDataTemplate;

        /// <summary>
        /// Set the template to use for detail
        /// </summary>
        /// <param name="detailTemplate"></param>
        public void SetDetailTemplate(DetailTemplate<TableItem> detailTemplate)
        {
            _detailTemplate = detailTemplate?.ChildContent;
        }

        private RenderFragment<TableItem> _detailTemplate;

        private SelectionType _selectionType;

        /// <summary>
        /// Select Type: None, Single or Multiple
        /// </summary>
        [Parameter]
        public SelectionType SelectionType
        {
            get { return _selectionType; }
            set
            {
                _selectionType = value;
                if (_selectionType == SelectionType.None)
                {
                    SelectedItems.Clear();
                }
                else if (_selectionType == SelectionType.Single && SelectedItems.Count > 1)
                {
                    SelectedItems.RemoveRange(1, SelectedItems.Count - 1);
                }
                StateHasChanged();
            }
        }

        /// <summary>
        /// Contains Selected Items
        /// </summary>
        [Parameter]
        public List<TableItem> SelectedItems { get; set; } = new List<TableItem>();

        /// <summary>
        /// Action performed when the row is clicked.
        /// </summary>
        [Parameter]
        public Action<TableItem> RowClickAction { get; set; }

        /// <summary>
        /// Handles the onclick action for table rows.
        /// This allows the RowClickAction to be optional.
        /// </summary>
        private void OnRowClickHandler(TableItem tableItem)
        {
            try
            {
                RowClickAction?.Invoke(tableItem);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "RowClickAction threw an exception: {0}", ex);
            }

            switch (SelectionType)
            {
                case SelectionType.None:
                    return;
                case SelectionType.Single:
                    SelectedItems.Clear();
                    SelectedItems.Add(tableItem);
                    break;
                case SelectionType.Multiple:
                    if (SelectedItems.Contains(tableItem))
                        SelectedItems.Remove(tableItem);
                    else
                        SelectedItems.Add(tableItem);
                    break;
            }
        }

        /// <summary>
        /// Add custom row to current table
        /// </summary>
        /// <param name="customRow">custom row to add</param>
        public void AddCustomRow(CustomRow<TableItem> customRow)
        {
            CustomRows.Add(customRow);
        }

        /// <summary>
        /// Shows Search Bar above the table
        /// </summary>
        [Parameter]
        public bool ShowSearchBar { get; set; }

        /// <summary>
        /// Hide invisible columns bar above the table
        /// </summary>
        [Parameter]
        public bool DisableInvisibleColumnsBar { get; set; }
        
        private bool ShowInvisibleColumnsBar => !DisableInvisibleColumnsBar && Columns.Any(column => !column.Visible);

        /// <summary>
        /// Show or hide table footer. Hide by default.
        /// </summary>
        [Parameter]
        public bool ShowFooter { get; set; }

        /// <summary>
        /// Set Table Page Size
        /// </summary>
        /// <param name="pageSize"></param>
        public async Task SetPageSizeAsync(int pageSize)
        {
            PageSize = pageSize;
            await UpdateAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Show table child content at the top of the table.
        /// </summary>
        [Parameter]
        public bool ShowChildContentAtTop { get; set; }

    }
}
