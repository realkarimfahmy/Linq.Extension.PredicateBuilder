using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Linq.Extension.PredicateBuilder
{
    public static class PredicateBuilder
    {
        public static Func<T, bool> Compile<T>(List<SearchFilter> filters) where T : class
        {
            var predicate = PredicateBuilder.True<T>();

            foreach (var item in filters)
            {
                predicate = predicate.And(GetPredicate<T>(item.PropertyName, item.Operation, item.Value));
            }
            return predicate.Compile();

        }

        private static Expression<Func<T, bool>> GetPredicate<T>(string modelPropertyName, OperatorComparer OperatorComparer, object data) where T : class
        {
            ParameterExpression parameterExp = Expression.Parameter(typeof(T), "t");
            MemberExpression member = Expression.PropertyOrField(parameterExp, modelPropertyName.Split('.').First());
            foreach (var innerMember in modelPropertyName.Split('.').Skip(1))
            {
                member = Expression.PropertyOrField(member, innerMember);
            }
            if (member.Type.BaseType.ToString() == "System.Enum")
            {
                data = Int32.Parse(data.ToString());
                string name = Enum.GetName(member.Type, data);
                data = Enum.Parse(member.Type, name, false);
            }
            else if (OperatorComparer != OperatorComparer.IsIn)
            {
                switch (member.Type.ToString())
                {
                    case "System.Nullable`1[System.Decimal]":
                        data = data.ToString().ToNullableDecimal();
                        break;
                    case "System.Nullable`1[System.Double]":
                        data = data.ToString().ToNullableDouble();
                        break;
                    case "System.Nullable`1[System.Float]":
                        data = data.ToString().ToNullableFloat();
                        break;
                    case "System.Nullable`1[System.DateTime]":
                        data = data.ToString().ToNullableDateTime();
                        break;
                    case "System.Nullable`1[System.Int16]":
                        data = data.ToString().ToNullableInt16();
                        break;
                    case "System.Nullable`1[System.Int32]":
                        data = data.ToString().ToNullableInt32();
                        break;
                    case "System.Nullable`1[System.Int64]":
                        data = data.ToString().ToNullableInt64();
                        break;
                    case "System.Nullable`1[System.UInt16]":
                        data = data.ToString().ToNullableUInt16();
                        break;
                    case "System.Nullable`1[System.UInt32]":
                        data = data.ToString().ToNullableUInt32();
                        break;
                    case "System.Nullable`1[System.UInt64]":
                        data = data.ToString().ToNullableUInt64();
                        break;
                    case "System.Nullable`1[System.Boolean]":
                        data = data.ToString().ToNullableBoolean();
                        break;
                    case "System.Decimal":
                        data = decimal.Parse(data.ToString());
                        break;
                    case "System.Double":
                        data = double.Parse(data.ToString());
                        break;
                    case "System.Float":
                        data = float.Parse(data.ToString());
                        break;
                    case "System.DateTime":
                        data = DateTime.Parse(data.ToString());
                        break;
                    case "System.Int16":
                        data = Int16.Parse(data.ToString());
                        break;
                    case "System.Int32":
                        data = Int32.Parse(data.ToString());
                        break;
                    case "System.Int64":
                        data = Int64.Parse(data.ToString());
                        break;
                    case "System.UInt16":
                        data = UInt16.Parse(data.ToString());
                        break;
                    case "System.UInt32":
                        data = UInt32.Parse(data.ToString());
                        break;
                    case "System.UInt64":
                        data = UInt64.Parse(data.ToString());
                        break;
                    case "System.Byte":
                        data = Byte.Parse(data.ToString());
                        break;
                    case "System.Boolean":
                        data = Boolean.Parse(data.ToString());
                        break;
                }
            }
            ConstantExpression valuetoCheck;
            if (OperatorComparer == OperatorComparer.IsIn)
            {
                valuetoCheck = Expression.Constant(data, GetListType(member.Type));
            }
            else
            {
                valuetoCheck = Expression.Constant(data, member.Type);
            }
            Expression expression = GetExpression<T>(OperatorComparer, member, valuetoCheck);
            Expression<Func<T, bool>> predicate = Expression.Lambda<Func<T, bool>>(expression, new ParameterExpression[] { parameterExp });
            return predicate;
        }

        private static Expression GetExpression<T>(OperatorComparer OperatorComparer, MemberExpression member, ConstantExpression valuetoCheck) where T : class
        {
            Expression expression;
            switch (OperatorComparer)
            {
                case OperatorComparer.Equal:
                    expression = Equals<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.NotEqual:
                    expression = NotEquals<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.Less:
                    expression = Less<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.LessOrEqual:
                    expression = LessOrEqual<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.Greater:
                    expression = More<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.GreaterOrEqual:
                    expression = MoreorEqual<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.BeginsWith:
                    expression = BeginsWith<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.DoesNotBeginWith:
                    expression = NotBeginsWith<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.IsIn:
                    expression = IsIn<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.IsNotIn:
                    expression = NotContains<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.EndsWith:
                    expression = EndsWith<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.DoesNotEndWith:
                    expression = NotEndsWith<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.Contains:
                    expression = Contains<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.DoesNotContain:
                    expression = NotContains<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.IsNull:
                    expression = IsNull<T>(member, valuetoCheck);
                    break;
                case OperatorComparer.IsNotNull:
                    expression = IsNotNull<T>(member, valuetoCheck);
                    break;
                default:
                    expression = Expression<Func<T, bool>>.Equal(member, valuetoCheck);
                    break;
            }
            return expression;
        }

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<T, bool>> False<T>()
        {
            return f => false;
        }

        public static Expression<Func<T, bool>> True<T>()
        {
            return f => true;
        }

        public static IList CreateList(Type type)
        {
            Type genericListType = typeof(List<>).MakeGenericType(type);
            return ((IList)Activator.CreateInstance(genericListType));
        }

        public static Type GetListType(Type type)
        {
            return CreateList(type).GetType();
        }

        private static Expression BeginsWith<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            MethodInfo toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
            Expression toLowerCall = Expression.Call(member, toLowerMethodInfo);

            MethodInfo containsMethodInfo = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
            ConstantExpression constantExpression = Expression.Constant(valuetoCheck.Value.ToString().ToLower(), typeof(string));
            return Expression.Call(toLowerCall, containsMethodInfo, constantExpression);
        }

        private static Expression Contains<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            MethodInfo toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
            Expression toLowerCall = Expression.Call(member, toLowerMethodInfo);

            MethodInfo containsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            ConstantExpression constantExpression = Expression.Constant(valuetoCheck.Value.ToString().ToLower(), typeof(string));
            return Expression.Call(toLowerCall, containsMethodInfo, constantExpression);

        }

        private static Expression IsIn<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            MethodInfo method = GetListType(member.Type).GetMethod("Contains", new[] { member.Type });
            return Expression<Func<T, bool>>.Call(valuetoCheck, method, member);
        }

        private static Expression EndsWith<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            MethodInfo toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
            Expression toLowerCall = Expression.Call(member, toLowerMethodInfo);

            MethodInfo containsMethodInfo = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
            ConstantExpression constantExpression = Expression.Constant(valuetoCheck.Value.ToString().ToLower(), typeof(string));
            return Expression.Call(toLowerCall, containsMethodInfo, constantExpression);

        }

        private static Expression Equals<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            return Expression<Func<T, bool>>.Equal(member, valuetoCheck);
        }

        private static Expression IsNotNull<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            return Expression<Func<T, bool>>.NotEqual(member, Expression.Constant(null, member.Type));
        }

        private static Expression IsNull<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            return Expression<Func<T, bool>>.Equal(member, Expression.Constant(null, member.Type));
        }

        private static Expression Less<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            return Expression<Func<T, bool>>.LessThan(member, valuetoCheck);
        }

        private static Expression LessOrEqual<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            return Expression<Func<T, bool>>.LessThanOrEqual(member, valuetoCheck);
        }

        private static Expression More<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            return Expression<Func<T, bool>>.GreaterThan(member, valuetoCheck);
        }

        private static Expression MoreorEqual<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            return Expression<Func<T, bool>>.GreaterThanOrEqual(member, valuetoCheck);
        }

        private static Expression NotBeginsWith<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            MethodInfo toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
            Expression toLowerCall = Expression.Call(member, toLowerMethodInfo);

            MethodInfo containsMethodInfo = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
            ConstantExpression constantExpression = Expression.Constant(valuetoCheck.Value.ToString().ToLower(), typeof(string));
            return Expression.Not(Expression<Func<T, bool>>.Call(toLowerCall, containsMethodInfo, constantExpression));
        }

        private static Expression NotContains<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            MethodInfo toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
            Expression toLowerCall = Expression.Call(member, toLowerMethodInfo);

            MethodInfo containsMethodInfo = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
            ConstantExpression constantExpression = Expression.Constant(valuetoCheck.Value.ToString().ToLower(), typeof(string));
            return Expression.Not(Expression<Func<T, bool>>.Call(toLowerCall, containsMethodInfo, constantExpression));

        }

        private static Expression NotEndsWith<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            MethodInfo toLowerMethodInfo = typeof(string).GetMethod("ToLower", new Type[] { });
            Expression toLowerCall = Expression.Call(member, toLowerMethodInfo);

            MethodInfo containsMethodInfo = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
            ConstantExpression constantExpression = Expression.Constant(valuetoCheck.Value.ToString().ToLower(), typeof(string));
            return Expression.Not(Expression<Func<T, bool>>.Call(toLowerCall, containsMethodInfo, constantExpression));

        }

        private static Expression NotEquals<T>(MemberExpression member, ConstantExpression valuetoCheck)
        {
            return Expression<Func<T, bool>>.NotEqual(member, valuetoCheck);
        }

        private static UInt64? ToNullableUInt64(this string s)
        {
            UInt64 i;
            if (UInt64.TryParse(s, out i)) return i;
            return null;
        }

        private static UInt32? ToNullableUInt32(this string s)
        {
            UInt32 i;
            if (UInt32.TryParse(s, out i)) return i;
            return null;
        }

        private static UInt16? ToNullableUInt16(this string s)
        {
            UInt16 i;
            if (UInt16.TryParse(s, out i)) return i;
            return null;
        }

        private static Int64? ToNullableInt64(this string s)
        {
            Int64 i;
            if (Int64.TryParse(s, out i)) return i;
            return null;
        }

        private static Int32? ToNullableInt32(this string s)
        {
            Int32 i;
            if (Int32.TryParse(s, out i)) return i;
            return null;
        }

        private static Int16? ToNullableInt16(this string s)
        {
            Int16 i;
            if (Int16.TryParse(s, out i)) return i;
            return null;
        }

        private static float? ToNullableFloat(this string s)
        {
            float i;
            if (float.TryParse(s, out i)) return i;
            return null;
        }

        private static double? ToNullableDouble(this string s)
        {
            double i;
            if (double.TryParse(s, out i)) return i;
            return null;
        }

        private static Decimal? ToNullableDecimal(this string s)
        {
            decimal i;
            if (Decimal.TryParse(s, out i)) return i;
            return null;
        }

        private static Boolean? ToNullableBoolean(this string s)
        {
            bool i;
            if (Boolean.TryParse(s, out i)) return i;
            return null;
        }

        private static DateTime? ToNullableDateTime(this string s)
        {
            DateTime i;
            if (DateTime.TryParse(s, out i)) return i;
            return null;
        }
    }
}