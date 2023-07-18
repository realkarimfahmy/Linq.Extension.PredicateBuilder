namespace Linq.Extension.PredicateBuilder
{
    public class QueryResult<T>
    {
        public List<Search> Search { get; set; }

        public IEnumerable<T> Items { get; set; }

        public Pager Pager { get; set; }

        public int PageNumber { get; set; }
    }
}
