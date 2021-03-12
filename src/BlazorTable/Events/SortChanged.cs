namespace BlazorTable.Events
{
    public class SortChanged<TableItem>
    {
        public IColumn<TableItem> Column { get; set; }
    }
}