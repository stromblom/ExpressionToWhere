using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Stromblom.ExpressionToWhere
{
    public static class SqlExpressionVisitor
    {
        public static List<(string, Comparison, object, Operator)> CreateFromExpression(Expression node, Operator @operator = Operator.None)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Lambda:
                    return LambdaVisit((LambdaExpression)node);
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    return AndOrVisit((BinaryExpression)node, @operator);
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    return EqualVisit((BinaryExpression)node, @operator);
                case ExpressionType.MemberAccess:
                    return MemberAccessVisit((MemberExpression)node, @operator);
                case ExpressionType.Not:
                    return NotVisit((UnaryExpression)node, @operator);
                case ExpressionType.Call:
                    return CallVisit((MethodCallExpression)node, @operator);
                default:
                    throw new Exception("Node type not supported.");
            }
        }

        static List<(string, Comparison, object, Operator)> LambdaVisit(LambdaExpression node)
        {
            return CreateFromExpression(node.Body);
        }

        static List<(string, Comparison, object, Operator)> AndOrVisit(BinaryExpression node, Operator precedingOperator)
        {
            var result = new List<(string, Comparison, object, Operator)>();
            var @operator = Operator.And;

            if (node.NodeType == ExpressionType.OrElse)
            {
                @operator = Operator.Or;
            }

            List<(string, Comparison, object, Operator)> leftResult = CreateFromExpression(node.Left, precedingOperator);
            result.AddRange(leftResult);

            List<(string, Comparison, object, Operator)> rightResult = CreateFromExpression(node.Right, @operator);
            result.AddRange(rightResult);

            return result;
        }

        static List<(string, Comparison, object, Operator)> EqualVisit(BinaryExpression node, Operator @operator)
        {
            var left = (MemberExpression)node.Left;

            var name = left.Member.Name;

            object value;
            if (node.Right is ConstantExpression right)
            {
                value = right.Value;
            }
            else
            {
                value = Expression.Lambda<Func<object>>(
                    Expression.Convert(node.Right, typeof(object))).Compile().Invoke();
            }

            var comparison = Comparison.Equal;

            if (node.NodeType == ExpressionType.NotEqual)
            {
                comparison = Comparison.NotEqual;
            }

            return new List<(string, Comparison, object, Operator)> { (name, comparison, value, @operator) };
        }

        static List<(string, Comparison, object, Operator)> MemberAccessVisit(MemberExpression node, Operator @operator, Comparison comparison = Comparison.Equal)
        {
            var name = node.Member.Name;

            var value = GetValueFromExpression(node);

            return new List<(string, Comparison, object, Operator)> { (name, comparison, value, @operator) };
        }

        private static List<(string, Comparison, object, Operator)> NotVisit(UnaryExpression node, Operator @operator)
        {
            return MemberAccessVisit((MemberExpression)node.Operand, @operator, Comparison.NotEqual);
        }

        private static List<(string, Comparison, object, Operator)> CallVisit(MethodCallExpression node, Operator @operator)
        {
            throw new NotImplementedException();
        }

        private static object GetValueFromExpression(MemberExpression node)
        {
            object value;

            if (node.Type == typeof(bool))
            {
                value = true;
            }
            else if (node.Type.IsValueType)
            {
                value = Activator.CreateInstance(node.Type);
            }
            else
            {
                value = Expression.Lambda<Func<object>>(
                    Expression.Convert(node, typeof(object))).Compile().Invoke();
            }

            return value;
        }
    }
}