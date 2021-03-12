using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorTable.Interfaces;
using Microsoft.AspNetCore.Components;

namespace BlazorTable.Addons.Parts
{
    public partial class CheckboxList<TItem>
    {
        private int _debounceMilliseconds = 350;
        private int _minCharsLength = 2;
        
        //Data for the Checkbox   
        [Parameter] public IEnumerable<TItem> Data { get; set; }  
        // The field to be shown adjacent to checkbox  
        [Parameter] public RenderFragment<TItem> DisplayField { get; set; }  
        // CSS Style for the Checkbox container   
        [Parameter] public string Style { get; set; } = "height: 13rem;margin-top: 0.125rem;overflow-y: scroll;";
        // Event triggered when hints filter changed 
        [Parameter] public Func<string, Task> OnFilterChanged { get; set; }  
        // The array which contains the list of selected checkboxs   
        [Parameter] public List<TItem> SelectedValues { get; set; } 

        private string FilterText { get; set; } = default;

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

        private async Task RefreshFilterValues(string item)
        {
            var onChangedTask = OnFilterChanged?.Invoke(item);
            if(onChangedTask is null)
                return;

            await onChangedTask.ConfigureAwait(false);
        }
    }
}