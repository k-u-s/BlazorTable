using System;
using System.Threading.Tasks;
using BlazorTable.Components.ServerSide;

namespace BlazorTable.Interfaces
{
    public interface IHintsLoader<THintItem>
    {
        public Task<SearchHintsResult<THintItem>> LoadHintsAsync(SearchHints parameters);
    }
}