using System.Collections;
using System.Linq.Expressions;
using Hall_rent.Context;
using Hall_rent.Entity;
using Hall_rent.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace Hall_rent.Tests.Support;

internal static class TestAsyncQueryable
{
    public static Mock<DbSet<T>> CreateDbSet<T>(IEnumerable<T> data) where T : class
    {
        var query = new TestAsyncEnumerable<T>(data);
        var mock = new Mock<DbSet<T>>();

        mock.As<IQueryable<T>>().Setup(x => x.Provider).Returns(query.Provider);
        mock.As<IQueryable<T>>().Setup(x => x.Expression).Returns(query.Expression);
        mock.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(query.ElementType);
        mock.As<IEnumerable<T>>().Setup(x => x.GetEnumerator()).Returns(() => query.GetEnumerator());
        mock.As<IEnumerable>().Setup(x => x.GetEnumerator()).Returns(() => query.GetEnumerator());
        mock.As<IAsyncEnumerable<T>>().Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) => query.GetAsyncEnumerator(ct));

        return mock;
    }

    public static Mock<AppDbContext> CreateContext(
        IEnumerable<HallEntity> halls,
        IEnumerable<HallBookingEntity> bookings)
    {
        var hallSet = CreateDbSet(halls);
        var bookingSet = CreateDbSet(bookings);
        var options = new DbContextOptionsBuilder<AppDbContext>().Options;
        var context = new Mock<AppDbContext>(options) { CallBase = true };

        context.Setup(x => x.Set<HallEntity>()).Returns(hallSet.Object);
        context.Setup(x => x.Set<HallBookingEntity>()).Returns(bookingSet.Object);

        return context;
    }

    private sealed class TestAsyncEnumerable<T> : IQueryable<T>, IAsyncEnumerable<T>
    {
        private readonly Expression _expression;
        private readonly TestAsyncQueryProvider _provider;

        public TestAsyncEnumerable(IEnumerable<T> source)
        {
            _expression = Expression.Constant(source.AsQueryable());
            _provider = new TestAsyncQueryProvider(source.AsQueryable().Provider);
        }

        public TestAsyncEnumerable(Expression expression, TestAsyncQueryProvider provider)
        {
            _expression = expression;
            _provider = provider;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken))
        {
            return new TestAsyncEnumerator<T>(GetEnumerator());
        }

        public Type ElementType => typeof(T);
        public Expression Expression => _expression;
        public IQueryProvider Provider => _provider;

        public IEnumerator<T> GetEnumerator()
        {
            return _provider.Execute<IEnumerable<T>>(_expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private sealed class TestAsyncQueryProvider : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().First();
            var type = typeof(TestAsyncEnumerable<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(type, expression, this)!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression, this);
        }

        public object? Execute(Expression expression)
        {
            return _inner.Execute(new SpecificationQueryRewriter().Visit(expression)!);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(new SpecificationQueryRewriter().Visit(expression)!);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Execute<TResult>(expression);
        }
    }

    private sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class SpecificationQueryRewriter : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) &&
                node.Method.Name == nameof(Queryable.Any) &&
                node.Arguments.Count == 2 &&
                node.Arguments[1] is MethodCallExpression specificationCall &&
                specificationCall.Method.DeclaringType == typeof(Specification) &&
                specificationCall.Method.Name == nameof(Specification.OverlapsBooking))
            {
                var source = Visit(node.Arguments[0]);
                var predicate = BuildOverlapsPredicate(specificationCall.Arguments);
                return Expression.Call(node.Method, source, Expression.Quote(predicate));
            }

            return base.VisitMethodCall(node);
        }

        private static Expression<Func<HallBookingEntity, bool>> BuildOverlapsPredicate(
            IReadOnlyList<Expression> arguments)
        {
            var booking = Expression.Parameter(typeof(HallBookingEntity), "b");
            var hallId = arguments[0];
            var from = arguments[1];
            var to = arguments[2];

            var body = Expression.AndAlso(
                Expression.AndAlso(
                    Expression.Equal(
                        Expression.Property(booking, nameof(HallBookingEntity.HallId)),
                        hallId),
                    Expression.LessThan(
                        Expression.Property(booking, nameof(HallBookingEntity.From)),
                        to)),
                Expression.GreaterThan(
                    Expression.Property(booking, nameof(HallBookingEntity.To)),
                    from));

            return Expression.Lambda<Func<HallBookingEntity, bool>>(body, booking);
        }
    }
}
