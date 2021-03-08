using System;
using System.Threading.Tasks;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Interfaces
{
    public interface IHintsLoader<HintItem>
    {
        public Task<SearchHintsResult<HintItem>> LoadHintsAsync(SearchHints parameters);
    }
}