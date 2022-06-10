using P8D.Infrastructure.Common.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace P8D.Infrastructure.Helpes
{
    public static class QueryHelper
    {
        public static PagedList<T> SortAndPaginationDynamic<T>(IQueryable<T> Data, SortAndPaginationModel sortAndPaginationModel)
        {
            string matchedPropertyName = sortAndPaginationModel.DefaulPropNameForOrder;
            var requestPropertyName = !string.IsNullOrEmpty(sortAndPaginationModel.PropToOrder) ? sortAndPaginationModel.PropToOrder.ToLower() : string.Empty;
            var propName = typeof(T).GetProperties().Select(p => p.Name).FirstOrDefault(x => x.ToLower() == requestPropertyName);
            if (!string.IsNullOrEmpty(propName))
            {
                matchedPropertyName = propName;
            }
            var param = Expression.Parameter(typeof(T));
            var memberAccess = Expression.Property(param, matchedPropertyName);
            var convertedMemberAccess = Expression.Convert(memberAccess, typeof(object));
            var orderPredicate = Expression.Lambda<Func<T, object>>(convertedMemberAccess, param);
            var dataOrder = sortAndPaginationModel.IsDesc ? Data.OrderByDescending(orderPredicate) : Data.OrderBy(orderPredicate);
            return new PagedList<T>(dataOrder,
                sortAndPaginationModel.Offset ?? CommonConstants.Config.DEFAULT_SKIP,
                sortAndPaginationModel.Limit ?? CommonConstants.Config.DEFAULT_TAKE);
        }
    }
}
