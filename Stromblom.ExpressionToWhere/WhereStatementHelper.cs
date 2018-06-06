﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Stromblom.ExpressionToWhere
{
    public class WhereStatementHelper
    {
        public static string GetWhereStatementAndParametersFromExpression<TSaga>(
            Expression<Func<TSaga, bool>> expression)
        {
            List<(string, Comparison, object, Operator)> columnsAndValues = SqlExpressionVisitor.CreateFromExpression(expression);

            if (!columnsAndValues.Any())
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            sb.Append("WHERE");

            var i = 0;
            foreach (var (name, comparison, value, @operator) in columnsAndValues)
            {
                switch (@operator)
                {
                    case Operator.And:
                        sb.Append(" AND");
                        break;
                    case Operator.Or:
                        sb.Append(" OR");
                        break;
                }

                var valueName = $"@value{i}";
                sb.Append($" {name}");

                switch (comparison)
                {
                    case Comparison.Equal:
                        sb.Append(" = ");
                        break;
                    case Comparison.NotEqual:
                        sb.Append(" != ");
                        break;
                    case Comparison.In:
                        sb.Append(" IN ");
                        break;
                    case Comparison.NotIn:
                        sb.Append(" NOT IN ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                sb.Append(valueName);

                i++;
            }

            return sb.ToString();
        }
    }
}