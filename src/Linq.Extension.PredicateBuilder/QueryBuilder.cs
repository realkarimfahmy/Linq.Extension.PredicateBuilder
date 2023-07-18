﻿using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
namespace Linq.Extension.PredicateBuilder
{
    /// <summary>
    /// Generic IQueryable filter implementation.  Based upon configuration of FilterRules 
    /// mapping to the data source.  When applied, acts as an advanced filter mechanism.
    /// </summary>
    public static class QueryBuilder
    {
        /// <summary>
        /// Gets or sets a value indicating whether incoming dates in the filter should be parsed as UTC.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [parse dates as UTC]; otherwise, <c>false</c>.
        /// </value>
        public static bool ParseDatesAsUtc { get; set; }

       
        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="useIndexedProperty">Whether or not to use indexed property</param>
        /// <param name="indexedPropertyName">The indexable property to use</param>
        /// <returns>Filtered IQueryable</returns>
        public static IQueryable<T> BuildQuery<T>(this IEnumerable<T> queryable, IFilterRule filterRule, bool useIndexedProperty = false, string indexedPropertyName = null)
        {
            string parsedQuery;
            return BuildQuery(queryable.AsQueryable(), filterRule, out parsedQuery, useIndexedProperty, indexedPropertyName);
        }

        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="useIndexedProperty">Whether or not to use indexed property</param>
        /// <param name="indexedPropertyName">The indexable property to use</param>
        /// <returns>Filtered IQueryable</returns>
        public static IQueryable<T> BuildQuery<T>(this IList<T> queryable, IFilterRule filterRule, bool useIndexedProperty = false, string indexedPropertyName = null)
        {
            string parsedQuery;
            return BuildQuery(queryable.AsQueryable(), filterRule, out parsedQuery, useIndexedProperty, indexedPropertyName);
        }

        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="useIndexedProperty">Whether or not to use indexed property</param>
        /// <param name="indexedPropertyName">The indexable property to use</param>
        /// <returns>Filtered IQueryable</returns>
        public static IQueryable<T> BuildQuery<T>(this IQueryable<T> queryable, IFilterRule filterRule, bool useIndexedProperty = false, string indexedPropertyName = null)
        {
            string parsedQuery;
            return BuildQuery(queryable, filterRule, out parsedQuery, useIndexedProperty, indexedPropertyName);
        }

        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules. 
        /// Returns the string representation for diagnostic purposes.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="parsedQuery">The parsed query.</param>
        /// <param name="useIndexedProperty">Whether or not to use indexed property</param>
        /// <param name="indexedPropertyName">The indexable property to use</param>
        /// <returns>Filtered IQueryable.</returns>
        public static IQueryable<T> BuildQuery<T>(this IQueryable<T> queryable, IFilterRule filterRule, out string parsedQuery, bool useIndexedProperty = false, string indexedPropertyName = null)
        {
            return BuildQuery(queryable, filterRule, new PredicateBuilderOptions { UseIndexedProperty = useIndexedProperty, IndexedPropertyName = indexedPropertyName }, out parsedQuery);
        }

        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules. 
        /// Returns the string representation for diagnostic purposes.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="options">The options to use when building the expression</param>
        /// <returns>Filtered IQueryable.</returns>
        public static IQueryable<T> BuildQuery<T>(this IQueryable<T> queryable, IFilterRule filterRule, PredicateBuilderOptions options)
        {
            string parsedQuery;
            return BuildQuery(queryable, filterRule, options, out parsedQuery);
        }

        /// <summary>
        /// Gets the filtered collection after applying the provided filter rules. 
        /// Returns the string representation for diagnostic purposes.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="options">The options to use when building the expression</param>
        /// <param name="parsedQuery">The parsed query.</param>
        /// <returns>Filtered IQueryable.</returns>
        public static IQueryable<T> BuildQuery<T>(this IQueryable<T> queryable, IFilterRule filterRule, PredicateBuilderOptions options, out string parsedQuery)
        {
            var expression = BuildExpressionLambda<T>(filterRule, options, out parsedQuery);

            if (expression == null)
            {
                return queryable;
            }

            var whereCallExpression = Expression.Call(
                typeof(Queryable),
                "Where",
                new[] { queryable.ElementType },
                queryable.Expression,
                expression);

            var filteredResults = queryable.Provider.CreateQuery<T>(whereCallExpression);

            return filteredResults;
        }

        /// <summary>
        /// Gets the filtered collection as paginated result after applying the provided filter rules.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <param name="queryable">The queryable.</param>
        /// <param name="predicate">The predicate model.</param>
        /// <returns>QueryResult<T></returns>
        public static QueryResult<T> ToPaged<T>(this IEnumerable<T> queryable, PredicateBuilderInvokerBase<T> predicate)
        {
            int pageNumber = 0;
            int pageSize = 0;

            if (predicate.PageNumber == 0) { pageNumber = 1; } else { pageNumber = predicate.PageNumber; }
            if (predicate.PageSize == 0) { pageSize = 10; } else { pageSize = predicate.PageSize; }

            var numberOfRecords = queryable.Count();
            var numberOfPages = GetPaggingCount(numberOfRecords, pageSize);

            var pager = new Pager
            {
                NumberOfPages = numberOfPages,
                CurrentPage = pageNumber,
                TotalRecords = numberOfRecords
            };

            var countFrom = _countFrom(pageSize, pageNumber);

            var resultSet = new QueryResult<T>
            {
                Items = queryable.Skip(countFrom).Take(pageSize).ToList(),
                PageNumber = pageNumber,
                Pager = pager,
                Search = predicate.Search
            };

            return resultSet;
        }

        private static readonly Func<int, int, int> _countFrom =
            (pageSize, pageNumber) => pageNumber == 1 ? 0 : (pageSize * pageNumber) - pageSize;

        private static int GetPaggingCount(int count, int pageSize)
        {
            var extraCount = count % pageSize > 0 ? 1 : 0;
            return (count < pageSize) ? 1 : (count / pageSize) + extraCount;
        }

        /// <summary>
        /// Builds a predicate that returns whether an input test object passes the filter rule.
        /// </summary>
        /// <typeparam name="T">The generic type of the input object to test.</typeparam>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="options">The options to use when building the expression</param>
        /// <returns>A predicate function implementing the filter rule</returns>
        public static Func<T, bool> BuildPredicate<T>(this IFilterRule filterRule, PredicateBuilderOptions options)
        {
            string parsedQuery;
            return BuildPredicate<T>(filterRule, options, out parsedQuery);
        }

        /// <summary>
        /// Builds a predicate that returns whether an input test object passes the filter rule.
        /// </summary>
        /// <typeparam name="T">The generic type of the input object to test.</typeparam>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="parsedQuery">The parsed query.</param>
        /// <param name="options">The options to use when building the expression</param>
        /// <returns>A predicate function implementing the filter rule</returns>
        public static Func<T, bool> BuildPredicate<T>(this IFilterRule filterRule, PredicateBuilderOptions options, out string parsedQuery)
        {
            var expression = BuildExpressionLambda<T>(filterRule, options, out parsedQuery);

            if (expression == null)
            {
                return _ => true;
            }

            return expression.Compile();
        }

        /// <summary>
        /// Builds an expression lambda for the filter rule.
        /// </summary>
        /// <typeparam name="T">The generic type of the input object to test.</typeparam>
        /// <param name="filterRule">The filter rule.</param>
        /// <param name="parsedQuery">The parsed query.</param>
        /// <param name="options">The options to use when building the expression</param>
        /// <returns>An expression lambda that implements the filter rule</returns>
        public static Expression<Func<T, bool>> BuildExpressionLambda<T>(this IFilterRule filterRule, PredicateBuilderOptions options, out string parsedQuery)
        {
            if (filterRule == null)
            {
                parsedQuery = "";
                return null;
            }

            var pe = Expression.Parameter(typeof(T), "item");

            var expressionTree = BuildExpressionTree<T>(pe, filterRule, options);
            if (expressionTree == null)
            {
                parsedQuery = "";
                return null;
            }

            parsedQuery = expressionTree.ToString();

            return Expression.Lambda<Func<T, bool>>(expressionTree, pe);

        }

        private static Expression BuildExpressionTree<T>(ParameterExpression pe, IFilterRule rule, PredicateBuilderOptions options)
        {

            if (rule.Rules != null && rule.Rules.Any())
            {
                var expressions =
                    rule.Rules.Select(childRule => BuildExpressionTree<T>(pe, childRule, options))
                        .Where(expression => expression != null)
                        .ToList();

                var expressionTree = expressions.First();

                var counter = 1;
                while (counter < expressions.Count)
                {
                    expressionTree = rule.Condition == RuleCondition.OR
                        ? Expression.OrElse(expressionTree, expressions[counter])
                        : Expression.AndAlso(expressionTree, expressions[counter]);
                    counter++;
                }

                return expressionTree;
            }
            if (rule.Field != null)
            {
                Type type = GetCSharpType<T>(rule.Field);

                if (options.UseIndexedProperty)
                {
                    var propertyExp = Expression.Property(pe, options.IndexedPropertyName, Expression.Constant(rule.Field));
                    return BuildOperatorExpression(propertyExp, rule, options, type);
                }
                else
                {
                    var propertyList = rule.Field.Split('.');
                    if (propertyList.Length > 1)
                    {
                        var propertyCollectionEnumerator = propertyList.AsEnumerable().GetEnumerator();
                        return BuildNestedExpression(pe, propertyCollectionEnumerator, rule, options, type);
                    }
                    else
                    {
                        var propertyExp = Expression.Property(pe, rule.Field);
                        return BuildOperatorExpression(propertyExp, rule, options, type);
                    }
                }
            }
            return null;
        }

        public static System.Type GetCSharpType<T>(string modelPropertyName)
        {
            Type type;
            ParameterExpression parameterExp = Expression.Parameter(typeof(T), "t");
            MemberExpression member = Expression.PropertyOrField(parameterExp, modelPropertyName.Split('.').First());
            foreach (var innerMember in modelPropertyName.Split('.').Skip(1))
            {
                member = Expression.PropertyOrField(member, innerMember);
            }
            return member.Type;

        }

        private static Expression BuildNestedExpression(Expression expression, IEnumerator<string> propertyCollectionEnumerator, IFilterRule rule, PredicateBuilderOptions options, Type type)
        {
            while (propertyCollectionEnumerator.MoveNext())
            {
                var propertyName = propertyCollectionEnumerator.Current;
                var property = expression.Type.GetProperty(propertyName);
                expression = Expression.Property(expression, property);

                var propertyType = property.PropertyType;
                var enumerable = propertyType.GetInterface("IEnumerable`1");
                // If the filter tries to access the Dictionary content
                if (IsDictionary(propertyType) && propertyCollectionEnumerator.MoveNext())
                {
                    var key = propertyCollectionEnumerator.Current;
                    var indexExpr = Expression.Constant(key);

                    var getItemMethod = propertyType.GetMethod("get_Item");
                    expression = Expression.Call(expression, getItemMethod!, indexExpr);
                    // recursively build the body of the lambda expression for the nested properties
                    var body = BuildNestedExpression(expression, propertyCollectionEnumerator, rule, options, type);
                    return body;
                }
                if (propertyType != typeof(string) && enumerable != null)
                {
                    var elementType = enumerable.GetGenericArguments()[0];
                    var predicateFnType = typeof(Func<,>).MakeGenericType(elementType, typeof(bool));
                    var parameterExpression = Expression.Parameter(elementType);

                    Expression body = BuildNestedExpression(parameterExpression, propertyCollectionEnumerator, rule, options, type);
                    var predicate = Expression.Lambda(predicateFnType, body, parameterExpression);
                    var queryable = Expression.Call(typeof(Queryable), "AsQueryable", new[] { elementType }, expression);

                    if (options.NullCheckNestedCLRObjects)
                    {
                        var notnull = Expression.NotEqual(expression, Expression.Constant(null, typeof(object)));
                        return Expression.AndAlso(notnull, Expression.Call(
                            typeof(Queryable),
                            "Any",
                            new[] { elementType },
                            queryable,
                            predicate
                        ));
                    }
                    else
                    {
                        return Expression.Call(
                            typeof(Queryable),
                            "Any",
                            new[] { elementType },
                            queryable,
                            predicate
                        );
                    }

                }
                if (options.NullCheckNestedCLRObjects && !expression.Type.IsValueType && propertyType != typeof(string))
                {
                    var notnull = IsNotNull(expression);
                    Expression body = BuildNestedExpression(expression, propertyCollectionEnumerator, rule, options, type);
                    return Expression.AndAlso(notnull, body);
                }
            }

            return BuildOperatorExpression(expression, rule, options, type);
        }

        private static bool IsDictionary(Type type)
        {
            if (type.GetInterface("IDictionary`2") is not null)
            {
                var genericTypes = type.GetGenericArguments();
                var keyType = genericTypes[0];
                if (keyType != typeof(string))
                {
                    throw new NotSupportedException("Non string key types are not supported");
                }

                return true;
            }

            return false;
        }

        private static Expression BuildOperatorExpression(Expression propertyExp, IFilterRule rule, PredicateBuilderOptions options, Type type)
        {
            Expression expression;
            OperatorComparer oper = rule.Operator;

            switch (oper)
            {
                case OperatorComparer.In:
                    expression = In(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.NotIn:
                    expression = NotIn(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.Equal:
                    expression = Equals(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.NotEqual:
                    expression = NotEquals(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.Between:
                    expression = Between(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.NotBetween:
                    expression = NotBetween(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.Less:
                    expression = LessThan(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.LessOrEqual:
                    expression = LessThanOrEqual(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.Greater:
                    expression = GreaterThan(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.GreaterOrEqual:
                    expression = GreaterThanOrEqual(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.BeginsWith:
                    expression = BeginsWith(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.DoesNotBeginWith:
                    expression = NotBeginsWith(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.Contains:
                    expression = Contains(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.DoesNotContain:
                    expression = NotContains(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.EndsWith:
                    expression = EndsWith(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.DoesNotEndWith:
                    expression = NotEndsWith(type, rule.Value, propertyExp, options);
                    break;
                case OperatorComparer.IsEmpty:
                    expression = IsEmpty(propertyExp);
                    break;
                case OperatorComparer.IsNotEmpty:
                    expression = IsNotEmpty(propertyExp);
                    break;
                case OperatorComparer.IsNull:
                    expression = IsNull(propertyExp);
                    break;
                case OperatorComparer.IsNotNull:
                    expression = IsNotNull(propertyExp);
                    break;

                default:
                    //custom operators support
                    var operators = options.Operators;
                    if (operators == null || operators.Count() <= 0)
                        throw new Exception($"Unknown expression operator: {rule.Operator}");

                    var customOperator = (from p in operators where p.Operator == oper select p).FirstOrDefault();
                    if (customOperator != null)
                    {
                        expression = customOperator.GetExpression(type, rule, propertyExp, options);
                    }
                    else
                    {
                        throw new Exception($"Unknown expression operator: {rule.Operator}");
                    }
                    break;
            }
            return expression;
        }

        public static List<ConstantExpression> GetConstants(Type type, object value, bool isCollection, PredicateBuilderOptions options)
        {
            if (type == typeof(DateOnly))
                type = typeof(DateTime);
            if (type == typeof(DateTime) && (options.ParseDatesAsUtc || ParseDatesAsUtc))
            {
                DateTime tDate;
                if (isCollection)
                {
                    if (!(value is string) && value is IEnumerable list)
                    {
                        var constants = new List<ConstantExpression>();

                        foreach (object item in list)
                        {
                            var date = DateTime.TryParse(item.ToString().Trim(), options.CultureInfo,
                                            DateTimeStyles.AdjustToUniversal, out tDate)
                                            ? (DateTime?)
                                                tDate
                                            : null;
                            constants.Add(Expression.Constant(date, type));
                        }

                        return constants;
                    }
                    else
                    {
                        var vals =
                            value.ToString().Split(new[] { ",", "[", "]", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                                .Select(
                                    p =>
                                        DateTime.TryParse(p.Trim(), options.CultureInfo,
                                            DateTimeStyles.AdjustToUniversal, out tDate)
                                            ? (DateTime?)
                                                tDate
                                            : null).Select(p =>
                                                Expression.Constant(p, type));
                        return vals.ToList();
                    }
                }
                else
                {
                    if (value is Array items) value = items.GetValue(0);
                    return new List<ConstantExpression>()
                    {
                        Expression.Constant(DateTime.TryParse(value.ToString().Trim(), options.CultureInfo,
                            DateTimeStyles.AdjustToUniversal, out tDate)
                            ? (DateTime?)
                                tDate
                            : null)
                    };
                }
            }
            else
            {
                if (isCollection)
                {
                    var tc = TypeDescriptor.GetConverter(type);
                    if (type == typeof(string))
                    {
                        if (!(value is string) && value is IEnumerable list)
                        {
                            var expressions = new List<ConstantExpression>();

                            foreach (object item in list)
                            {
                                expressions.Add(Expression.Constant(tc.ConvertFromString(null, options.CultureInfo, item.ToString()), type));
                            }

                            return expressions;
                        }
                        else
                        {
                            var bracketSplit = value.ToString().Split(new[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                            var vals =
                                    bracketSplit.SelectMany(v => v.Split(new[] { ",", "\r\n" }, StringSplitOptions.None))
                                    .Select(p => tc.ConvertFromString(null, options.CultureInfo, p.Trim())).Select(p =>
                                        Expression.Constant(p, type));
                            return vals.Distinct().ToList();
                        }
                    }
                    else
                    {
                        if (!(value is string) && value is IEnumerable list)
                        {
                            var expressions = new List<ConstantExpression>();

                            foreach (object item in list)
                            {
                                expressions.Add(Expression.Constant(tc.ConvertFromString(null, options.CultureInfo, item.ToString()), type));
                            }

                            return expressions;
                        }
                        else
                        {
                            var vals =
                            value.ToString().Split(new[] { ",", "[", "]", "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                                .Select(p => tc.ConvertFromString(null, options.CultureInfo, p.Trim())).Select(p =>
                                    Expression.Constant(p, type));
                            return vals.ToList();
                        }
                    }
                }
                else
                {
                    var tc = TypeDescriptor.GetConverter(type);
                    if (value is Array items) value = items.GetValue(0);

                    return new List<ConstantExpression>
                    {
                        Expression.Constant(tc.ConvertFromString(null, options.CultureInfo, value.ToString().Trim()))
                    };
                }
            }



        }

        #region Expression Types

        /// <summary>
        /// Checks expression for null as a short circuit
        /// </summary>
        /// <param name="propertyExp">Expression to be checked.</param>
        /// <returns>Expression determining null state</returns>
        public static Expression GetNullCheckExpression(Expression propertyExp)
        {
            var isNullable = !propertyExp.Type.IsValueType || Nullable.GetUnderlyingType(propertyExp.Type) != null;

            if (isNullable)
            {
                return Expression.NotEqual(propertyExp,
                    Expression.Constant(propertyExp.Type.GetDefaultValue(), propertyExp.Type));

            }
            return Expression.Constant(true, typeof(bool));
        }

        private static Expression IsNull(Expression propertyExp)
        {
            var isNullable = !propertyExp.Type.IsValueType || Nullable.GetUnderlyingType(propertyExp.Type) != null;

            if (isNullable)
            {
                var someValue = Expression.Constant(null, propertyExp.Type);

                Expression exOut = Expression.Equal(propertyExp, someValue);

                return exOut;
            }

            return Expression.Constant(false, typeof(bool));
        }

        private static Expression IsNotNull(Expression propertyExp)
        {
            return Expression.Not(IsNull(propertyExp));
        }

        private static Expression IsEmpty(Expression propertyExp)
        {
            var someValue = Expression.Constant(0, typeof(int));

            var nullCheck = GetNullCheckExpression(propertyExp);

            Expression exOut;

            if (IsGenericList(propertyExp.Type))
            {
                exOut = Expression.Property(propertyExp, propertyExp.Type.GetProperty("Count"));

                exOut = Expression.AndAlso(nullCheck, Expression.Equal(exOut, someValue));
            }
            else if (IsGuid(propertyExp.Type))
            {
                someValue = Expression.Constant(Guid.Empty, propertyExp.Type);

                exOut = Expression.AndAlso(nullCheck, Expression.Equal(propertyExp, someValue));
            }
            else
            {
                exOut = Expression.Property(propertyExp, typeof(string).GetProperty("Length"));

                exOut = Expression.AndAlso(nullCheck, Expression.Equal(exOut, someValue));
            }

            return exOut;
        }

        private static Expression IsNotEmpty(Expression propertyExp)
            => IsGuid(propertyExp.Type)
                ? Expression.AndAlso(GetNullCheckExpression(propertyExp), Expression.NotEqual(propertyExp, Expression.Constant(Guid.Empty, propertyExp.Type)))
                : Expression.Not(IsEmpty(propertyExp));

        private static Expression Contains(Type type, object value, Expression propertyExp,
            PredicateBuilderOptions buildExpressionOptions)
        {
            if (value is Array items) value = items.GetValue(0);
            var nullCheck = GetNullCheckExpression(propertyExp);
            MethodCallExpression propertyExpString = null;

            if (ShouldConvertToString(propertyExp.Type))
            {
                propertyExpString = Expression.Call(propertyExp, propertyExp.Type.GetMethod("ToString", Type.EmptyTypes));
                type = typeof(string);
            }

            MethodInfo method;
            Expression argument;
            var exOut = propertyExpString ?? propertyExp;
            if (IsDictionary(propertyExp.Type))
            {
                method = exOut.Type.GetMethod("ContainsKey", new[] { type });
                argument = Expression.Constant(value.ToString(), typeof(string));
            }
            else
            {
                method = exOut.Type.GetMethod("Contains", new[] { type });
                GetExpressionsOperands(buildExpressionOptions, exOut, value, out exOut, out argument);
            }

            exOut = Expression.AndAlso(nullCheck, Expression.Call(exOut, method, argument));

            return exOut;
        }

        private static void GetExpressionsOperands(PredicateBuilderOptions options, Expression property,
            object value, out Expression operand, out Expression argument)
        {
            var strValue = value.ToString();
            operand = property;

            if (!options.StringCaseSensitiveComparison)
            {
                strValue = strValue.ToLower();
                operand = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
            }

            argument = Expression.Constant(strValue, typeof(string));
        }

        private static void GetExpressionsOperands(PredicateBuilderOptions options, Expression property,
            Expression value, out Expression operand, out Expression argument)
        {
            operand = property;

            if (!options.StringCaseSensitiveComparison)
            {
                value = Expression.Call(value, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
                operand = Expression.Call(property, typeof(string).GetMethod("ToLower", Type.EmptyTypes));
            }

            argument = value;
        }

        private static Expression NotContains(Type type, object value, Expression propertyExp,
            PredicateBuilderOptions options)
        {
            return Expression.Not(Contains(type, value, propertyExp, options));
        }

        private static Expression EndsWith(Type type, object value, Expression propertyExp,
            PredicateBuilderOptions options)
        {
            if (value is Array items) value = items.GetValue(0);

            var nullCheck = GetNullCheckExpression(propertyExp);
            MethodCallExpression propertyExpString = null;

            if (ShouldConvertToString(propertyExp.Type))
            {
                propertyExpString = Expression.Call(propertyExp, propertyExp.Type.GetMethod("ToString", Type.EmptyTypes));
                type = typeof(string);
            }
            var method = (propertyExpString ?? propertyExp).Type.GetMethod("EndsWith", new[] { type });

            GetExpressionsOperands(options, propertyExpString ?? propertyExp, value, out var exOut, out var argument);
            exOut = Expression.AndAlso(nullCheck, Expression.Call(exOut, method, argument));
            return exOut;
        }

        private static Expression NotEndsWith(Type type, object value, Expression propertyExp,
            PredicateBuilderOptions buildExpressionOptions)
        {
            return Expression.Not(EndsWith(type, value, propertyExp, buildExpressionOptions));
        }

        private static Expression BeginsWith(Type type, object value, Expression propertyExp,
            PredicateBuilderOptions options)
        {
            if (value is Array items) value = items.GetValue(0);

            var nullCheck = GetNullCheckExpression(propertyExp);

            MethodCallExpression propertyExpString = null;

            if (ShouldConvertToString(propertyExp.Type))
            {
                propertyExpString = Expression.Call(propertyExp, propertyExp.Type.GetMethod("ToString", Type.EmptyTypes));
                type = typeof(string);
            }
            var method = (propertyExpString ?? propertyExp).Type.GetMethod("StartsWith", new[] { type });

            GetExpressionsOperands(options, propertyExpString ?? propertyExp, value, out var exOut, out var argument);
            exOut = Expression.AndAlso(nullCheck, Expression.Call(exOut, method, argument));
            return exOut;
        }

        private static Expression NotBeginsWith(Type type, object value, Expression propertyExp,
            PredicateBuilderOptions options)
        {
            return Expression.Not(BeginsWith(type, value, propertyExp, options));
        }



        private static Expression NotEquals(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            return Expression.Not(Equals(type, value, propertyExp, options));
        }

        private struct DateOnly { }
        private static Expression Equals(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            Expression someValue = GetConstants(type, value, false, options).First();

            Expression exOut;
            if (type == typeof(string))
            {
                var nullCheck = GetNullCheckExpression(propertyExp);

                MethodCallExpression propertyExpString = null;

                if (ShouldConvertToString(propertyExp.Type))
                {
                    propertyExpString = Expression.Call(propertyExp, propertyExp.Type.GetMethod("ToString", Type.EmptyTypes));
                }

                GetExpressionsOperands(options, propertyExpString ?? propertyExp, someValue, out exOut, out var argument);
                exOut = Expression.AndAlso(nullCheck, Expression.Equal(exOut, argument));
            }
            else if (type == typeof(DateOnly))
            {
                if (Nullable.GetUnderlyingType(propertyExp.Type) != null)
                {
                    exOut = Expression.Property(propertyExp, typeof(DateTime?).GetProperty("Value"));
                    exOut = Expression.Equal(
                        Expression.Property(exOut, typeof(DateTime).GetProperty("Date")),
                        Expression.Convert(someValue, typeof(DateTime)));

                    exOut = Expression.AndAlso(GetNullCheckExpression(propertyExp), exOut);
                }
                else
                {
                    exOut = Expression.Equal(
                        Expression.Property(propertyExp, typeof(DateTime).GetProperty("Date")),
                        Expression.Convert(someValue, propertyExp.Type));
                }
            }
            else
            {
                PerformCasting(propertyExp, someValue, type, out propertyExp, out someValue);
                exOut = Expression.Equal(propertyExp, someValue);
            }

            return exOut;
        }

        private static Expression LessThan(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            Expression someValue = GetConstants(type, value, false, options).First();
            PerformCasting(propertyExp, someValue, type, out propertyExp, out someValue);
            Expression exOut = Expression.LessThan(propertyExp, someValue);

            return exOut;
        }

        private static Expression LessThanOrEqual(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            Expression someValue = GetConstants(type, value, false, options).First();
            PerformCasting(propertyExp, someValue, type, out propertyExp, out someValue);

            return Expression.LessThanOrEqual(propertyExp, someValue);
        }

        private static Expression GreaterThan(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            Expression someValue = GetConstants(type, value, false, options).First();
            PerformCasting(propertyExp, someValue, type, out propertyExp, out someValue);

            return Expression.GreaterThan(propertyExp, someValue);
        }

        private static Expression GreaterThanOrEqual(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            Expression someValue = GetConstants(type, value, false, options).First();
            PerformCasting(propertyExp, someValue, type, out propertyExp, out someValue);
            Expression exOut = Expression.GreaterThanOrEqual(propertyExp, someValue);
            return exOut;
        }

        private static Expression Between(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            var someValue = GetConstants(type, value, true, options);
            PerformCasting(propertyExp, someValue[0], type, out var castedProperty, out var greaterThanValue);
            Expression exBelow = Expression.GreaterThanOrEqual(castedProperty, greaterThanValue);
            PerformCasting(propertyExp, someValue[1], type, out castedProperty, out var lessThanValue);
            Expression exAbove = Expression.LessThanOrEqual(castedProperty, lessThanValue);

            return Expression.And(exBelow, exAbove);
        }

        private static Expression NotBetween(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            return Expression.Not(Between(type, value, propertyExp, options));
        }

        private static Expression In(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            var someValues = GetConstants(type, value, true, options);

            var nullCheck = GetNullCheckExpression(propertyExp);

            if (IsGenericList(propertyExp.Type))
            {
                var genericType = propertyExp.Type.GetGenericArguments().First();
                var method = propertyExp.Type.GetMethod("Contains", new[] { genericType });
                Expression exOut = default;

                bool isDtOnly = type == typeof(DateOnly);
                MethodCallExpression listCallExp = null;

                if (isDtOnly)
                {
                    MethodCallExpression whereCallExp = null;
                    var isNullable = Nullable.GetUnderlyingType(genericType) != null;

                    if (isNullable)
                    {
                        var paramExp1 = Expression.Parameter(typeof(DateTime?), "d");
                        var memberExpression1 = GetNullCheckExpression(paramExp1);
                        var lambdaExp1 = Expression.Lambda(memberExpression1, paramExp1);
                        whereCallExp = Expression.Call(ReflectionHelpers.WhereMethod.MakeGenericMethod(typeof(DateTime?)), propertyExp, lambdaExp1);
                    }

                    var paramExp = Expression.Parameter(genericType, "d");
                    var memberExpression = isNullable
                        ? Expression.Property(Expression.Property(paramExp, genericType.GetProperty("Value")), typeof(DateTime).GetProperty("Date"))
                        : Expression.Property(paramExp, genericType.GetProperty("Date"));
                    var lambdaExp = Expression.Lambda(memberExpression, paramExp);

                    var selectCallExp = Expression.Call(ReflectionHelpers.SelectMethod.MakeGenericMethod(genericType, typeof(DateTime)), whereCallExp ?? propertyExp, lambdaExp);
                    listCallExp = Expression.Call(ReflectionHelpers.ToListMethod.MakeGenericMethod(typeof(DateTime)), selectCallExp);
                    exOut = Expression.Call(ReflectionHelpers.ContainsMethod.MakeGenericMethod(typeof(DateTime)), listCallExp, Expression.Convert(someValues[0], typeof(DateTime)));
                }
                else
                    exOut = Expression.Call(propertyExp, method, Expression.Convert(someValues[0], genericType));


                var counter = 1;

                while (counter < someValues.Count)
                {
                    MethodCallExpression methodCall = null;
                    if (isDtOnly)
                    {
                        methodCall = Expression.Call(
                            ReflectionHelpers.ContainsMethod.MakeGenericMethod(typeof(DateTime)),
                            listCallExp,
                            Expression.Convert(someValues[counter], typeof(DateTime)));
                    }
                    else
                    {
                        methodCall = Expression.Call(propertyExp, method, Expression.Convert(someValues[counter], genericType));
                    }

                    exOut = Expression.Or(exOut, methodCall);
                    counter++;
                }


                return Expression.AndAlso(nullCheck, exOut);
            }
            else
            {
                Expression exOut;

                if (someValues.Count > 1)
                {
                    if (type == typeof(string))
                    {
                        Expression propertyExpString = null;
                        if (ShouldConvertToString(propertyExp.Type))
                        {
                            propertyExpString = Expression.Call(propertyExp, propertyExp.Type.GetMethod("ToString", Type.EmptyTypes));
                        }

                        var property = propertyExpString ?? propertyExp;
                        GetExpressionsOperands(options, property, someValues[0], out var leftOperand, out var argument);
                        exOut = Expression.Equal(leftOperand, argument);
                        var counter = 1;
                        while (counter < someValues.Count)
                        {
                            GetExpressionsOperands(options, property, someValues[counter], out leftOperand, out argument);
                            exOut = Expression.Or(exOut, Expression.Equal(leftOperand, argument));
                            counter++;
                        }
                    }
                    else
                    {
                        PerformCasting(propertyExp, someValues[0], type, out var castedProperty, out var someValue);
                        exOut = Expression.Equal(castedProperty, someValue);
                        var counter = 1;
                        while (counter < someValues.Count)
                        {
                            PerformCasting(propertyExp, someValues[counter], type, out castedProperty, out someValue);
                            exOut = Expression.Or(exOut,
                                Expression.Equal(castedProperty, someValue));
                            counter++;
                        }
                    }
                }
                else
                {
                    if (type == typeof(string))
                    {
                        Expression propertyExpString = null;
                        if (ShouldConvertToString(propertyExp.Type))
                        {
                            propertyExpString = Expression.Call(propertyExp, propertyExp.Type.GetMethod("ToString", Type.EmptyTypes));
                        }
                        GetExpressionsOperands(options, propertyExpString ?? propertyExp, someValues.First(), out exOut, out var argument);
                        exOut = Expression.Equal(exOut, argument);
                    }
                    else
                    {
                        exOut = Equals(type, someValues.First(), propertyExp, options);
                    }
                }


                return Expression.AndAlso(nullCheck, exOut);
            }
        }

        private static Expression NotIn(Type type, object value, Expression propertyExp, PredicateBuilderOptions options)
        {
            return Expression.Not(In(type, value, propertyExp, options));
        }

        #endregion

        public static bool IsGenericList(this Type o)
        {
            var isGenericList = false;

            var oType = o;

            if (oType.IsGenericType && ((oType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                                        (oType.GetGenericTypeDefinition() == typeof(ICollection<>)) ||
                                        (oType.GetGenericTypeDefinition() == typeof(List<>)) ||
                                        (oType.GetGenericTypeDefinition() == typeof(HashSet<>))))
                isGenericList = true;

            return isGenericList;
        }

        public static bool IsGuid(this Type o) => o.UnderlyingSystemType.Name == "Guid" || Nullable.GetUnderlyingType(o)?.Name == "Guid";

        public static bool ShouldConvertToString(this Type o) => IsGuid(o) || o == typeof(object) || o.IsEnum;

        private static object GetDefaultValue(this Type type)
        {
            return type.GetTypeInfo().IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static void PerformCasting(Expression propertyExp, Expression constant, Type type, out Expression castedProperty, out Expression castedConstant)
        {
            castedProperty = propertyExp;
            castedConstant = constant;
            // if our type is a super class of the compared type, downcast our type
            // for example compare object to int
            if (type.IsSubclassOf(propertyExp.Type))
            {
                castedProperty = Expression.Convert(propertyExp, type);
            }
            // support nullables
            else // int is not a subclass of nullable int
            {
                castedConstant = Expression.Convert(constant, propertyExp.Type);
            }
        }

    }
}
