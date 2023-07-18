using System.Linq.Expressions;

namespace Linq.Extension.PredicateBuilder
{
    public interface IFilterOperator
    {
        /// <summary>
        /// Custom operator name
        /// </summary>
        OperatorComparer Operator { get; }

        /// <summary>
        /// Get Custom Expression
        /// </summary>
        /// <param name="type"></param>
        /// <param name="rule"></param>
        /// <param name="propertyExp"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Expression GetExpression(Type type, IFilterRule rule, Expression propertyExp, PredicateBuilderOptions options);
    }
}
