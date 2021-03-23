using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BlazorTable.Components.ClientSide;
using BlazorTable.Interfaces;
using LinqKit;
using Microsoft.Extensions.Logging;

namespace BlazorTable.Components.ServerSide
{
    internal class InMemoryDataLoader<TableItem> : IDataLoader<TableItem>
    {
        private PaginationResult<TableItem> Empty = new()
        {
            Top = 0,
            Skip = 0,
            Total = 0,
            Records = ArraySegment<TableItem>.Empty
        };

        private readonly FilterKnownHandlers<TableItem> _filterKnownHandlers;
        private readonly Table<TableItem> _table;
        private readonly ILogger _logger;

        public InMemoryDataLoader(Table<TableItem> table,
            ILogger<ITable<TableItem>> logger)
        {
            _table = table;
            _logger = logger;
            _filterKnownHandlers = table.FiltersKnown;
        }

        public Task<PaginationResult<TableItem>> LoadDataAsync(FilterData parameters)
        {
            if (_table.Items is null)
                return Task.FromResult(Empty);
            
            _logger.LogDebug($"Creating query for table items with {_table.Items.Count} records");
            var query = _table.Items.AsQueryable();
            if (parameters.OrderDirection != SortDirection.UnSet)
            {
                var sortColumn = _table.Columns.FirstOrDefault(el => el.Key == parameters.OrderBy);
                if (sortColumn is not null)
                {
                    _logger.LogDebug($"Sorting by column key {sortColumn.Key} with direction {parameters.OrderDirection}");
                    query = parameters.OrderDirection == SortDirection.Ascending
                        ? query.OrderBy(sortColumn.Field)
                        : query.OrderByDescending(sortColumn.Field);
                }
            }
            _logger.LogDebug($"Using global search value: {parameters.GlobalSearchQuery}");
            if (!string.IsNullOrEmpty(parameters.GlobalSearchQuery))
                query = query.Where(GlobalSearchQuery(parameters.GlobalSearchQuery));
            
            var filterableColumns = _table.Columns
                .Where(el => el.Filter is not null);
            foreach (var column in filterableColumns)
            {
                var filter = column.Filter;
                _logger.LogDebug($"Filtering by column key {column.Key} with parameters: {filter}");
                var filterHandler = _filterKnownHandlers.GetHandler(filter)
                    ?? throw new ArgumentNullException(nameof(filter));
                query = filterHandler.Filter(filter, column, query);
            }

            var totalQuery = query.Count();
            _logger.LogDebug($"Skipping {parameters.Skip} and taking {parameters.Top} values");
            if (parameters.Skip.HasValue)
                query = query.Skip(parameters.Skip.Value);

            if (parameters.Top.HasValue)
                query = query.Take(parameters.Top.Value);

            var records = query.ToList().AsReadOnly();
            _logger.LogDebug($"Returning {records.Count} records");
            return Task.FromResult(new PaginationResult<TableItem>
            {
                Records = records,
                Total = totalQuery,
                Skip = parameters.Skip ?? 0,
                Top = parameters.Top ?? records.Count,
            });
        }

        private Expression<Func<TableItem, bool>> GlobalSearchQuery(string value)
        {
            Expression<Func<TableItem, bool>> expression = null;

            foreach (string keyword in value.Trim().Split(" "))
            {
                Expression<Func<TableItem, bool>> tmp = null;

                foreach (var column in _table.Columns.Where(x => x.Field != null))
                {
                    var newQuery = Expression.Lambda<Func<TableItem, bool>>(
                        Expression.AndAlso(
                            column.Field.Body.CreateNullChecks(),
                            Expression.GreaterThanOrEqual(
                                Expression.Call(
                                    Expression.Call(column.Field.Body, "ToString", Type.EmptyTypes),
                                    typeof(string).GetMethod(nameof(string.IndexOf), new[] { typeof(string), typeof(StringComparison) }),
                                    new[] { Expression.Constant(keyword), Expression.Constant(StringComparison.OrdinalIgnoreCase) }),
                                Expression.Constant(0))),
                        column.Field.Parameters[0]);

                    if (tmp == null)
                        tmp = newQuery;
                    else
                        tmp = tmp.Or(newQuery);
                }

                if (expression == null)
                    expression = tmp;
                else
                    expression = expression.And(tmp);
            }

            return expression;
        }
    }
}