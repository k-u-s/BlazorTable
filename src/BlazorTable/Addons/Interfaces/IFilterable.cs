namespace BlazorTable.Interfaces
{
    public interface IFilterable<HintItem>
    {
        bool Contains(HintItem item);
    }
}