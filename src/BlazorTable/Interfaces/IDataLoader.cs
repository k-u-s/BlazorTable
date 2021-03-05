using System;
using System.Threading.Tasks;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Interfaces
{
    public interface IDataLoader<T>
    {
        public bool IsSearchHintSupported => false;

        public Task<PaginationResult<T>> LoadDataAsync(FilterData parameters);
        public Task<SearchHintsResult> GetHints(SearchHints parameters) => throw new NotImplementedException();
    }
}
