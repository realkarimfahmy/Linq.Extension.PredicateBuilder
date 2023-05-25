namespace Linq.Extension.PredicateBuilder
{
    public class SearchFilter
    {
        public string PropertyName { get; set; }
        public OperatorComparer Operation { get; set; }
        public object Value { get; set; }
    }
}
