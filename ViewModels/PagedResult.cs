namespace SystemMagazynu.ViewModels
{
    // Uniwersalny wynik stronicowany - do użycia w listach z paginacją.
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; }

        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling(TotalCount / (double)PageSize)
            : 0;

        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
