using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BlazorTable.Components.ClientSide;
using BlazorTable.Interfaces;
using LinqKit;

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
        private readonly Table<TableItem> _table;

        public InMemoryDataLoader(Table<TableItem> table)
        {
            _table = table;
        }

        public Task<PaginationResult<TableItem>> LoadDataAsync(FilterData parameters)
        {
            if (_table.Items is null)
                return Task.FromResult(Empty);
            
            var query = _table.Items.AsQueryable();
            if (parameters.OrderDirection != SortDirection.UnSet)
            {
                var sortColumn = _table.Columns.FirstOrDefault(el => el.Key == parameters.OrderBy);
                if (sortColumn is not null)
                {
                    query = parameters.OrderDirection == SortDirection.Ascending
                        ? query.OrderBy(sortColumn.Field)
                        : query.OrderByDescending(sortColumn.Field);
                }
            }
            if (!string.IsNullOrEmpty(parameters.GlobalSearchQuery))
                query = query.Where(GlobalSearchQuery(parameters.GlobalSearchQuery));
            
            var filters = _table.Columns
                .Where(el => el.Filter is not null)
                .Select(el => el.Filter);
            foreach (var filter in filters)
            {
                if (filter is not InMemoryFilterEntry<TableItem> inMemoryFilter)
                    throw new ArgumentException("Client side filter have to implement InMemoryFilterEntry.",
                        nameof(filter));
                
                query = inMemoryFilter.Filter(query);
            }
            
            if (parameters.Skip.HasValue)
                query = query.Skip(parameters.Skip.Value);

            if (parameters.Top.HasValue)
                query = query.Take(parameters.Top.Value);
            
            return Task.FromResult(new PaginationResult<TableItem>
            {
                Records = query,
                Total = _table.TotalCount,
                Skip = parameters.Skip ?? 0,
                Top = parameters.Top ?? _table.TotalCount,
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