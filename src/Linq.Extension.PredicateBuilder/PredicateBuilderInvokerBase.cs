namespace Linq.Extension.PredicateBuilder
{
    public class PredicateBuilderInvokerBase<T> : IResultSet<T>
    {
        public List<Search> Search { get; set; }

        public Pager Pager { get; set; }

        public IEnumerable<T> Items { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public PredicateBuilderFilterRule PredicateBuilderFilterRule
        {
            get
            {
                return new PredicateBuilderFilterRule { Condition = RuleCondition.And, Rules = QueryAdapter(this.Search), PageNumber = this.PageNumber, PageSize = this.PageSize };
            }
        }

        private static List<PredicateBuilderFilterRule> QueryAdapter(List<Search> searches)
        {
            var rules = new List<PredicateBuilderFilterRule>();
            if (searches != null)
            {
                foreach (var ruleItem in searches)
                {
                    var singleRule = new PredicateBuilderFilterRule
                    {
                        Condition = RuleCondition.And,
                        Field = ruleItem.Field.Value,
                        Operator = ruleItem.Operator.Value
                    };
                    if (ruleItem.Operator.Value == OperatorComparer.In)
                    {
                        string[] ids = ruleItem.Value.value.Split(',').ToArray<string>();
                        singleRule.Value = ids;
                    }
                    else
                    {
                        singleRule.Value = new string[] { ruleItem.Value.value, ruleItem.Value.value2 };
                    }
                    rules.Add(singleRule);
                }
            }
            return rules;
        }
    }

}
