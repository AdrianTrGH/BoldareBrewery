using BoldareBrewery.Application.Interfaces;
using BoldareBrewery.Application.UseCases.SearchBreweries;
using BoldareBrewery.Domain.Data.Entities;
using System;
using System.Linq.Expressions;

namespace BoldareBrewery.Application.Specifications;

public class BrewerySearchSpecification : BaseSpecification<BreweryEntity>
{
    public BrewerySearchSpecification(SearchBreweriesRequest request) : base(BuildCriteria(request))
    {
        // Apply sorting - distance sorting handled separately in strategy
        switch (request.SortBy?.ToLower())
        {
            case "name":
                ApplyOrderBy(b => b.Name);
                break;
            case "city":
                ApplyOrderBy(b => b.City);
                break;
            case "distance":
                // Distance sorting requires special handling with user location
                // Will be handled in the strategy layer
                ApplyOrderBy(b => b.Name); // Fallback
                break;
            default:
                ApplyOrderBy(b => b.Name);
                break;
        }

        // Apply pagination
        ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);
    }

    private static Expression<Func<BreweryEntity, bool>>? BuildCriteria(SearchBreweriesRequest request)
    {
        Expression<Func<BreweryEntity, bool>>? criteria = null;

        if (!string.IsNullOrEmpty(request.Search))
        {
            Expression<Func<BreweryEntity, bool>> searchCriteria = b =>
            b.Name.ToUpper().Contains(request.Search.ToUpper()) ||
            b.City.ToUpper().Contains(request.Search.ToUpper());
            criteria = criteria == null ? searchCriteria : CombineExpressions(criteria, searchCriteria, ExpressionType.AndAlso);
        }

        if (!string.IsNullOrEmpty(request.City))
        {
            Expression<Func<BreweryEntity, bool>> cityCriteria = b => b.City.ToUpper().Contains(request.City.ToUpper());
            criteria = criteria == null ? cityCriteria : CombineExpressions(criteria, cityCriteria, ExpressionType.AndAlso);
        }

        return criteria;
    }

    private static Expression<Func<T, bool>> CombineExpressions<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second,
        ExpressionType mergeWith)
    {
        var parameter = Expression.Parameter(typeof(T));
        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);
        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<T, bool>>(
            mergeWith == ExpressionType.AndAlso ? Expression.AndAlso(left!, right!) : Expression.OrElse(left!, right!),
            parameter);
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression _oldValue;
        private readonly Expression _newValue;

        public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}