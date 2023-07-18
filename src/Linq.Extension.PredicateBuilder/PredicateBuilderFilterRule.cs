namespace Linq.Extension.PredicateBuilder
{
    public class PredicateBuilderFilterRule : IFilterRule
    {
        /// <summary>
        /// Condition - acceptable values are "and" and "or".
        /// </summary>
        /// <value>
        /// The condition.
        /// </value>
        public RuleCondition Condition { get; set; }
        /// <summary>
        /// The name of the field that the filter applies to.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        public string Field { get; set; }
        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        public OperatorComparer Operator { get; set; }
        /// <summary>
        /// Gets or sets nested filter rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        public List<PredicateBuilderFilterRule> Rules { get; set; }
        /// <summary>
        /// Gets or sets the value of the filter.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string[] Value { get; set; }

        IEnumerable<IFilterRule> IFilterRule.Rules => Rules;

        object IFilterRule.Value => Value;

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
