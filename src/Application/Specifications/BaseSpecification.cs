using BoldareBrewery.Application.Interfaces;
using System.Linq.Expressions;

namespace BoldareBrewery.Application.Specifications
{
    public abstract class BaseSpecification<T> : ISpecification<T>
        where T : class
    {
        public Expression<Func<T, bool>>? Criteria { get; private set; }
        public List<Expression<Func<T, object>>> Includes { get; } = [];
        public Expression<Func<T, object>>? OrderBy { get; private set; }
        public Expression<Func<T, object>>? OrderByDescending { get; private set; }
        public int Take { get; private set; }
        public int Skip { get; private set; }
        public bool IsPagingEnabled { get; private set; }

        protected BaseSpecification()
        {
            Includes = [];
        }

        protected BaseSpecification(Expression<Func<T, bool>> criteria) : this()
        {
            Criteria = criteria;
        }

        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        protected virtual void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }  
    }
}
