namespace Linq.Extension.PredicateBuilder
{
    public interface IFilterRule
    {
        /// <summary>
        /// Condition - acceptable values are "And" and "OR".
        /// </summary>
        /// <value>
        /// The condition.
        /// </value>
        RuleCondition Condition { get; }
        /// <summary>
        /// The name of the field that the filter applies to.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        string Field { get; }
        /// <summary>
        /// Gets or sets the operator.
        /// </summary>
        /// <value>
        /// The operator.
        /// </value>
        OperatorComparer Operator { get; }
        /// <summary>
        /// Gets or sets nested filter rules.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        IEnumerable<IFilterRule> Rules { get; }
        /// <summary>
        /// Gets or sets the value of the filter.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        object Value { get; }
        /// <summary>
        /// Gets or sets the PageNumber of the filter.
        /// </summary>
        /// <value>
        /// The PageNumber.
        /// </PageNumber>
        int PageNumber { get; }
        /// <summary>
        /// Gets or sets the PageSize of the filter.
        /// </summary>
        /// <value>
        /// The PageNumber.
        /// </PageSize>
        int PageSize { get; }
    }
}
