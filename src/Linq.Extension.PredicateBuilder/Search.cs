namespace Linq.Extension.PredicateBuilder
{
    public class Search
    {
        public Field Field { get; set; }
        public Operator Operator { get; set; }
        public Value Value { get; set; }
    }
    public class Field
    {
        public string Label { get; set; }
        public string Value { get; set; }
    }

    public class Operator
    {
        public string Label { get; set; }
        public OperatorComparer Value { get; set; }
    }

    public class Value
    {
        public string Label { get; set; }
        public string value { get; set; }
        public string Label2 { get; set; }
        public string value2 { get; set; }
    }
}
