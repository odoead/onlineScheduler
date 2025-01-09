namespace Shared.Paging
{
    public class PagedList<T> where T : class
    {
        public MetaData MetaData { get; }
        public List<T> DataList { get; }
        private PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            MetaData = new MetaData
            {
                TotalCount = count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            DataList = items.ToList();
        }

        public static PagedList<T> ToPagedList(IEnumerable<T> source, int count, int pageNumber, int pageSize)
        {
            return new PagedList<T>(source, count, pageNumber, pageSize);
        }
    }
}
