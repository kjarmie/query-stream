using Application.Utilities.DataSource;

namespace QueryStream;

public class DataQuery
{
    /// <summary>
    /// Gets the zero-based index of the page to retrieve.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Gets the number of items to skip from the beginning of the data set.
    /// </summary>
    public int Skip { get; init; }

    public IEnumerable<SortDescriptor> Sorts { get; init; } = [];

    /// <summary>
    /// Gets the maximum number of items to retrieve.
    /// </summary>
    public int Take { get; init; }

    /// <summary>
    /// Gets the optional search query to filter the data.
    /// </summary>
    public string Search { get; init; }

    public DataQuery(int page, int skip, int take, IEnumerable<SortDescriptor> sorts, string search = "")
    {
        Page = page;
        Skip = skip;
        Take = take;
        Sorts = sorts;
        Search = search;
    }
}