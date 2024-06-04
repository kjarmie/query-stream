using System.Linq.Expressions;
using System.Reflection;
using Application.Utilities.DataSource;
using LanguageExt;
using LanguageExt.Common;

namespace QueryStream;

public static class DataQueryExtensions
{
    public static DataQueryResult<T> ToDataQueryResult<T>(this IEnumerable<T> enumerable, DataQuery request)
    {
        try
        {
            var seq = enumerable.ToSeq();
            var total = seq.Count();
            var filtered = seq.ApplySearch(request).Count();
            var data = seq.AsQueryable()
                .ApplySearch(request)
                .ApplySort(request)
                .ApplyPaging(request)
                .ToSeq();

            return new DataQueryResult<T>(request.Page, total, filtered, data, Option<Error>.None);

            // return new DataQueryResult<T>(request.Page, total, data.Count(), data, Option<Error>.None);
        }
        catch (Exception e)
        {
            return DataQueryResult<T>.Failed(Error.New(e));
        }
    }

    public static DataQueryResult<T> ToDataQueryResult<T>(this IQueryable<T> queryable, DataQuery request)
    {
        try
        {
            // var total = queryable.Count();
            // var data = queryable.Search(request.Search)
            //     .Skip(request.Skip)
            //     .Take(request.Take)
            //     .ToSeq();

            var total = queryable.Count();
            var filtered = queryable.ApplySearch(request).Count();
            var queried = queryable
                .ApplySearch(request)
                .ApplySort(request)
                .ApplyPaging(request);

            var data = queried.ToList().ToSeq();

            return new DataQueryResult<T>(request.Page, total, filtered, data, Option<Error>.None);
        }
        catch (Exception e)
        {
            return DataQueryResult<T>.Failed(Error.New(e));
        }
    }

    private static IQueryable<T> ApplyPaging<T>(this IQueryable<T> source, DataQuery request)
    {
        return request.Take == -1
            ? source
            : source
                .Skip(request.Skip)
                .Take(request.Take);
    }

    private static IEnumerable<T> ApplyPaging<T>(this IEnumerable<T> source, DataQuery request)
    {
        return request.Take == -1
            ? source
            : source
                .Skip(request.Skip)
                .Take(request.Take);
    }

    private static IQueryable<T> ApplySearch<T>(this IQueryable<T> source, DataQuery request)
    {
        if (string.IsNullOrWhiteSpace(request.Search))
            return source;
        
        List<Expression> expressions = new List<Expression>();

        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");

        MethodInfo contains_method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        foreach (PropertyInfo prop in typeof(T).GetProperties().Where(x => x.PropertyType == typeof(string)))
        {
            MemberExpression member_expression = Expression.PropertyOrField(parameter, prop.Name);

            ConstantExpression value_expression = Expression.Constant(request.Search, typeof(string));

            MethodCallExpression contains_expression =
                Expression.Call(member_expression, contains_method, value_expression);

            expressions.Add(contains_expression);
        }

        if (expressions.Count == 0)
            return source;

        Expression or_expression = expressions[0];

        expressions.Iter((exp) => { or_expression = Expression.OrElse(or_expression, exp); });

        Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(
            or_expression, parameter);

        var filtered = source.Where(expression);

        return filtered;
    }

    private static IEnumerable<T> ApplySearch<T>(this IEnumerable<T> source, DataQuery request)
    {
        List<Expression> expressions = new List<Expression>();

        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");

        MethodInfo contains_method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        foreach (PropertyInfo prop in typeof(T).GetProperties().Where(x => x.PropertyType == typeof(string)))
        {
            MemberExpression member_expression = Expression.PropertyOrField(parameter, prop.Name);

            ConstantExpression value_expression = Expression.Constant(request.Search, typeof(string));

            MethodCallExpression contains_expression =
                Expression.Call(member_expression, contains_method, value_expression);

            expressions.Add(contains_expression);
        }

        if (expressions.Count == 0)
            return source;

        Expression or_expression = expressions[0];

        expressions.Iter((exp) => { or_expression = Expression.OrElse(or_expression, exp); });

        Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(
            or_expression, parameter);

        return source.Where(expression.Compile());
    }


    private static IQueryable<T> ApplySort<T>(this IQueryable<T> source, QueryStream.DataQuery request)
    {
        var t = request.Sorts.Iter(s =>
        {
            var parameter = Expression.Parameter(typeof(T));
            var property = typeof(T).GetProperty(s.ColumnName, BindingFlags.IgnoreCase |  BindingFlags.Public | BindingFlags.Instance);
            if (property == null)
                throw new ArgumentException($"Property '{s.ColumnName}' not found on type '{typeof(T)}'");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            Expression expression = Type.GetTypeCode(property.PropertyType) switch
            {
                TypeCode.Boolean => Expression.Lambda<Func<T, bool>>(propertyAccess, parameter),
                TypeCode.Byte => Expression.Lambda<Func<T, byte>>(propertyAccess, parameter),
                TypeCode.Char => Expression.Lambda<Func<T, char>>(propertyAccess, parameter),
                TypeCode.String => Expression.Lambda<Func<T, string>>(propertyAccess, parameter),
                TypeCode.Int32 => Expression.Lambda<Func<T, int>>(propertyAccess, parameter),
                TypeCode.Int64 => Expression.Lambda<Func<T, long>>(propertyAccess, parameter),
                TypeCode.Decimal => Expression.Lambda<Func<T, decimal>>(propertyAccess, parameter),
                TypeCode.Double => Expression.Lambda<Func<T, double>>(propertyAccess, parameter),
                TypeCode.Object => Expression.Lambda<Func<T, object>>(propertyAccess, parameter),
                _ => throw new ArgumentException($"Property '{s.ColumnName}' has unhandled type '{property.PropertyType}'")
            };

            source = s.SortDirection == SortDirection.Ascending
                ? Queryable.OrderBy(source, (dynamic)expression)
                : Queryable.OrderByDescending(source, (dynamic)expression);
        });

        return source;
    }
}