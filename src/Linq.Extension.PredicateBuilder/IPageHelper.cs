namespace Linq.Extension.PredicateBuilder
{
    public interface IResultSet<T>
    {
        PredicateBuilderFilterRule PredicateBuilderFilterRule { get; }
        public List<Search> Search { get; }
        Pager Pager { get; }
        int PageNumber { get; }
        IEnumerable<T> Items { get; }
    }

    public class Pager
    {
        public int NumberOfPages { get; set; }

        public int CurrentPage { get; set; }

        public int TotalRecords { get; set; }
    }

}
