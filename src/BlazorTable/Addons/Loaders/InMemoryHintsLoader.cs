using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using BlazorTable.Components;
using BlazorTable.Components.ServerSide;
using BlazorTable.Interfaces;

namespace BlazorTable.Addons.Loaders
{
    public class InMemoryHintsLoader : IHintsLoader<object>
    {
        private SearchHintsResult<object> _emptyHints = new()
        {
            Skip = 0,
            Top = 0,
            Total = 0,
            Records = ArraySegment<object>.Empty
        };
        private readonly IReadOnlyCollection<object> _items;

        public InMemoryHintsLoader(IEnumerable<object> items)
        {
            _items = items.Distinct().ToImmutableSortedSet();
        }

        public Task<SearchHintsResult<object>> LoadHintsAsync([NotNull]SearchHints parameters)
        {
            var query = _items.AsQueryable();
            if(!string.IsNullOrEmpty(parameters.Hint))
                query = query.Where(el => FilterHints(el, parameters));
            
            if (parameters.Skip.HasValue)
                query = query.Skip(parameters.Skip.Value);

            if (parameters.Top.HasValue)
                query = query.Take(parameters.Top.Value);

            var records = query.ToList();
            return Task.FromResult(new SearchHintsResult<object>
            {
                Top = parameters.Top ?? records.Count,
                Skip = parameters.Skip ?? 0,
                Total = records.Count,
                Records = records
            });
        }

        private bool FilterHints(object el, [NotNull]SearchHints parameters)
            => el switch
            {
                null => false,
                IFilterable<string> filterable => filterable.Contains(parameters.Hint),
                string val => val.Contains(parameters.Hint, StringComparison.InvariantCultureIgnoreCase),
                _ => el.ToString().Contains(parameters.Hint, StringComparison.InvariantCultureIgnoreCase)
            };
    }
}