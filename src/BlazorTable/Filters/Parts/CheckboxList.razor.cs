using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorTable.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorTable.Filters.Parts
{
    public partial class CheckboxList<TItem>
    {
        private int _debounceMilliseconds = 350;
        private int _minCharsLength = 2;
        
        //Data for the Checkbox   
        [Parameter] public IEnumerable<TItem> Data { get; set; }  
        // The field to be shown adjacent to checkbox  
        [Parameter] public Func<TItem, string> TextField { get; set; }  
        // CSS Style for the Checkbox container   
        [Parameter] public string Style { get; set; } = "height: 12rem;overflow-y: scroll;";
        // CSS Style for the Checkbox container   
        [Parameter] public string FilterText { get; set; } = string.Empty;
        // Any childd content for the control (if needed)  
        [Parameter] public RenderFragment ChildContent { get; set; }  
        // The array which contains the list of selected checkboxs   
        [Parameter] public List<TItem> SelectedValues { get; set; } 

        private List<TItem> FilteredData { get; set; } = new List<TItem>();

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
            FilteredData = Data.ToList();
            if(!string.IsNullOrWhiteSpace(FilterText))
                RefreshFilterValues(FilterText);
        }

        public void CheckboxClicked(TItem selected, bool wasChecked)  
        {  
            if (wasChecked)  
            {  
                SelectedValues.Remove(selected);  
            }  
            else  
            {  
                SelectedValues.Add(selected);  
            }  
            StateHasChanged();  
        }

        public void SelectAll()
        {
            SelectedValues.Clear();
            SelectedValues.AddRange(Data);
            StateHasChanged();  
        }

        public void UnselectAll()
        {
            SelectedValues.Clear();
            StateHasChanged();  
        }

        private void RefreshFilterValues(string text)
        {
            FilterText = text;
            FilteredData = Data
                .Where(FilterValues)
                .ToList();
            StateHasChanged();
            
            bool FilterValues(TItem el)
            {
                return el switch
                {
                    string val => val.Contains(text, StringComparison.InvariantCultureIgnoreCase),
                    IFilterable filterable => filterable.Contains(text),
                    null => false,
                    _ => el.ToString()?.Contains(text, StringComparison.InvariantCultureIgnoreCase) == true
                };
            }
        }

        private bool CanFilterValues()
        {
            var curType = typeof(TItem);
            return typeof(IFilterable).IsAssignableFrom(curType)
                   || curType == typeof(string);
        }
    }
}