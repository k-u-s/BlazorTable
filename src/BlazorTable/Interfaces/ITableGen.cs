﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using BlazorTable.Components;
using BlazorTable.Events;
using BlazorTable.Interfaces;

namespace BlazorTable
{
    /// <summary>
    /// BlazorTable Interface
    /// </summary>
    /// <typeparam name="TableItem"></typeparam>
    public interface ITable<TableItem> : ITable
    {
        /// <summary>
        /// List of All Available Columns
        /// </summary>
        List<IColumn<TableItem>> Columns { get; }

        /// <summary>
        /// Adds a Column to the Table
        /// </summary>
        /// <param name="column"></param>
        void AddColumn(IColumn<TableItem> column);

        /// <summary>
        /// Removes a Column from the Table
        /// </summary>
        /// <param name="column"></param>
        void RemoveColumn(IColumn<TableItem> column);

        /// <summary>
        /// Collection to display in the table
        /// </summary>
        IReadOnlyCollection<TableItem> Items { get; set; }

        /// <summary>
        /// Service to use to query data server side
        /// </summary>
        public IDataLoader<TableItem> DataLoader { get; set; }
        
        /// <summary>
        /// Collection of filtered items
        /// </summary>
        IReadOnlyCollection<TableItem> FilteredItems { get; }

        /// <summary>
        /// Action performed when the row is clicked
        /// </summary>
        Action<TableItem> RowClickAction { get; set; }

        /// <summary>
        /// Callback triggered after sort order changes
        /// </summary>
        Action<SortChanged<TableItem>> OnSortChanged { get; set; }

        /// <summary>
        /// Callback triggered after filter changes
        /// </summary>
        Action<FilterChanged<TableItem>> OnFilterChanged { get; set; }

        /// <summary>
        /// Callback triggered after pagination changes
        /// </summary>
        Action<PaginationChanged<TableItem>> OnPaginationChanged { get; set; }
        
        /// <summary>
        /// Collection of selected items
        /// </summary>
        List<TableItem> SelectedItems { get; }

        /// <summary>
        /// Set the SetDetailTemplate for the table
        /// </summary>
        /// <param name="template"></param>
        void SetDetailTemplate(DetailTemplate<TableItem> template);

        /// <summary>
        /// Add custom row to table
        /// </summary>
        /// <param name="customRow">Custom row to add</param>
        void AddCustomRow(CustomRow<TableItem> customRow);
    }
}
