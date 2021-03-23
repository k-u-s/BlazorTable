namespace BlazorTable.Events
{
    public class PaginationChanged<TableItem>
    {
        public int PageRecords { get; set; }

        public int PageNumber { get; set; }
    }
}