using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace Stromblom.ExpressionToWhere.Tests
{
    public class SqlExpressionVisitorTests
    {
        [Test]
        public void CreateFromExpression_CanHandleEqualNodes_WithConstantValues()
        {
            // Arrange
            Expression<Func<TestType, bool>> filter = x => x.Name == "Fiskbullar";

            // Act
            var result = SqlExpressionVisitor.CreateFromExpression(filter).Single();

            // Assert
            Assert.That(result.Item1, Is.EqualTo(nameof(TestType.Name)));
            Assert.That(result.Item2, Is.EqualTo(Comparison.Equal));
            Assert.That(result.Item3, Is.EqualTo("Fiskbullar"));
            Assert.That(result.Item4, Is.EqualTo(Operator.None));
        }

        [Test]
        public void CreateFromExpression_CanHandleEqualNodes_WithBool()
        {
            // Arrange
            Expression<Func<TestType, bool>> filter = x => x.Completed;

            // Act
            var result = SqlExpressionVisitor.CreateFromExpression(filter).Single();

            // Assert
            Assert.That(result.Item1, Is.EqualTo(nameof(TestType.Completed)));
            Assert.That(result.Item2, Is.EqualTo(Comparison.Equal));
            Assert.That(result.Item3, Is.True);
            Assert.That(result.Item4, Is.EqualTo(Operator.None));
        }

        [Test]
        public void CreateFromExpression_CanHandleEqualNodes_WithNonConstantValues()
        {
            // Arrange
            var sagaId = Guid.NewGuid();
            Expression<Func<TestType, bool>> filter = x => x.Id == sagaId;

            // Act
            var result = SqlExpressionVisitor.CreateFromExpression(filter).Single();

            // Assert
            Assert.That(result.Item1, Is.EqualTo(nameof(TestType.Id)));
            Assert.That(result.Item2, Is.EqualTo(Comparison.Equal));
            Assert.That(result.Item3, Is.EqualTo(sagaId));
            Assert.That(result.Item4, Is.EqualTo(Operator.None));
        }

        [Test]
        public void CreateFromExpression_CanHandleAndAlsoNodes_WithNonConstantValues_AndBools()
        {
            // Arrange
            var sagaId = Guid.NewGuid();
            Expression<Func<TestType, bool>> filter = x => x.Id == sagaId && x.Completed;

            // Act
            var result = SqlExpressionVisitor.CreateFromExpression(filter);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));

            var first = result.First();
            Assert.That(first.Item1, Is.EqualTo(nameof(TestType.Id)));
            Assert.That(first.Item2, Is.EqualTo(Comparison.Equal));
            Assert.That(first.Item4, Is.EqualTo(Operator.None));
            Assert.That(first.Item3, Is.EqualTo(sagaId));

            var last = result.Last();
            Assert.That(last.Item1, Is.EqualTo(nameof(TestType.Completed)));
            Assert.That(last.Item2, Is.EqualTo(Comparison.Equal));
            Assert.That(last.Item4, Is.EqualTo(Operator.And));
            Assert.That(last.Item3, Is.True);
        }

        [Test]
        public void CreateFromExpression_CanHandleAndAlsoNodes_WithNestedAndAlso()
        {
            // Arrange
            var sagaId = Guid.NewGuid();
            Expression<Func<TestType, bool>> filter = x => x.Id == sagaId && x.Completed && x.Name == "Kebabsvarv";

            // Act
            var result = SqlExpressionVisitor.CreateFromExpression(filter);

            // Assert
            Assert.That(result.Count, Is.EqualTo(3));

            var first = result[0];
            Assert.That(first.Item1, Is.EqualTo(nameof(TestType.Id)));
            Assert.That(first.Item2, Is.EqualTo(Comparison.Equal));
            Assert.That(first.Item3, Is.EqualTo(sagaId));
            Assert.That(first.Item4, Is.EqualTo(Operator.None));

            var second = result[1];
            Assert.That(second.Item1, Is.EqualTo(nameof(TestType.Completed)));
            Assert.That(second.Item2, Is.EqualTo(Comparison.Equal));
            Assert.That(second.Item3, Is.True);
            Assert.That(second.Item4, Is.EqualTo(Operator.And));

            var third = result[2];
            Assert.That(third.Item1, Is.EqualTo(nameof(TestType.Name)));
            Assert.That(third.Item2, Is.EqualTo(Comparison.Equal));
            Assert.That(third.Item3, Is.EqualTo("Kebabsvarv"));
            Assert.That(third.Item4, Is.EqualTo(Operator.And));
        }

        [Test]
        public void PlaceHolder()
        {
            var sagaId = Guid.NewGuid();
            Expression<Func<TestType, bool>> filter = x => x.Id == sagaId || x.Completed && x.Name != "Kebabsvarv";

            var result = WhereStatementHelper.GetWhereStatementAndParametersFromExpression(filter);
        }
    }
}