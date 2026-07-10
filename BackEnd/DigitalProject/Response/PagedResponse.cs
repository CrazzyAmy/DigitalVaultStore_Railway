namespace DigitalProject.Response
{
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; }
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
        public bool HasPrev => Page > 1;
        public bool HasNext => Page < TotalPages;

    }
}
