namespace BlazorTable.Interfaces
{
    public interface IFilterable<THintItem>
    {
        bool Contains(THintItem item);
    }
}