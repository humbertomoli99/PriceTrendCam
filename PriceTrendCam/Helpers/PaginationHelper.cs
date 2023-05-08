namespace PriceTrendCam.Helpers;
public class PaginationHelper<T>
{
    private readonly List<T> collection;
    private readonly int itemsPerPage;

    public PaginationHelper(List<T> collection, int itemsPerPage)
    {
        this.collection = collection;
        this.itemsPerPage = itemsPerPage;
    }

    public int ItemCount => collection.Count;

    public int PageCount => (int)Math.Ceiling((double)ItemCount / itemsPerPage);

    public int PageItemCount(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= PageCount)
        {
            return -1;
        }

        if (pageIndex == PageCount - 1)
        {
            return ItemCount % itemsPerPage == 0 ? itemsPerPage : ItemCount % itemsPerPage;
        }

        return itemsPerPage;
    }

    public int PageIndex(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= ItemCount)
        {
            return -1;
        }

        return itemIndex / itemsPerPage;
    }

    public bool CanMoveToFirstPage
        => PageCount > 0 && PageIndex(0) != 0;

    public bool CanMoveToPreviousPage
        => PageCount > 0 && PageIndex(collection.Count - 1) != 0;

    public bool CanMoveToNextPage
        => PageCount > 0 && PageIndex(collection.Count - 1) != PageCount - 1;

    public bool CanMoveToLastPage
        => PageCount > 0 && PageIndex(collection.Count - 1) != PageCount - 1;
}