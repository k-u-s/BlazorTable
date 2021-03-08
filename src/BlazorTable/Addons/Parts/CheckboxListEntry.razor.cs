using System;
using Microsoft.AspNetCore.Components;

namespace BlazorTable.Addons.Parts
{
    public partial class CheckboxListEntry<TItem>
    {   
        // CSS Style for the Checkbox container   
        [Parameter] public string Style { get; set; }  
        [Parameter] public bool Checked { get; set; }  
        [Parameter] public TItem Value { get; set; }  
        [Parameter] public RenderFragment<TItem> DisplayField  { get; set; }  
        [Parameter] public Action<TItem, bool> OnClick { get; set; }  
    }
}