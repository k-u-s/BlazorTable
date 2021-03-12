namespace BlazorTable.Events
{
    public class FilterChanged<TableItem>
    {
        public IColumn<TableItem> Column { get; set; }
    }
}