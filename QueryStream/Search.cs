using System.Linq.Expressions;
using System.Reflection;

namespace Application.Utilities.DataSource;

public static class LinqExtensions
{
    public static IEnumerable<T> Search<T>(this IEnumerable<T> list, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return list;
        }

        var type = typeof(T);
        var props = type.GetProperties();

        return list.Where(item =>
            props
                .Any(prop => (prop.GetValue(item)?.ToString() ?? "").Contains(searchTerm)));
    }

    public static IQueryable<T> Search<T>(this IQueryable<T> source, string searchTerm)
    {
        IQueryable<T> query = source;

        List<Expression> expressions = new List<Expression>();

        ParameterExpression parameter = Expression.Parameter(typeof(T), "p");

        MethodInfo contains_method = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        foreach (PropertyInfo prop in typeof(T).GetProperties().Where(x => x.PropertyType == typeof(string)))
        {
            MemberExpression member_expression = Expression.PropertyOrField(parameter, prop.Name);

            ConstantExpression value_expression = Expression.Constant(searchTerm, typeof(string));

            MethodCallExpression contains_expression =
                Expression.Call(member_expression, contains_method, value_expression);

            expressions.Add(contains_expression);
        }

        if (expressions.Count == 0)
            return query;

        Expression or_expression = expressions[0];

        expressions.Iter((exp) =>
        {
            or_expression = Expression.OrElse(or_expression, exp);
        });

        Expression<Func<T, bool>> expression = Expression.Lambda<Func<T, bool>>(
            or_expression, parameter);

        return query.Where(expression);
    }
}