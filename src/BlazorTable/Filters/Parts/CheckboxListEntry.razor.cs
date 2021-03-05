using System;
using Microsoft.AspNetCore.Components;

namespace BlazorTable.Filters.Parts
{
    public partial class CheckboxListEntry<TItem>
    {   
        // CSS Style for the Checkbox container   
        [Parameter] public string Style { get; set; }  
        [Parameter] public bool Checked { get; set; }  
        [Parameter] public TItem Value { get; set; }  
        [Parameter] public string Text { get; set; }  
        [Parameter] public Action<TItem, bool> OnClick { get; set; }  
    }
}