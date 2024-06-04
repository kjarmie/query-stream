using LanguageExt;
using LanguageExt.Common;

namespace QueryStream;

/// <summary>
/// The result of a DataSourceRequest applied to a datasource.
/// </summary>
/// <typeparam name="T">The entity type of the datasource.</typeparam>
public class DataQueryResult<T>
{
    public DataQueryResult(int page, int total, int filtered, IEnumerable<T> data, Option<Error> error)
    {
        Page = page;
        Total = total;
        Filtered = filtered;
        Data = data;
        Error = error;
    }

    public static DataQueryResult<T> Failed(Error error)
    {
        return new DataQueryResult<T>(0, 0, 0, [], Option<Error>.Some(error));
    }

    /// <summary>
    /// The data to be returned.
    /// </summary>
    public IEnumerable<T> Data { get; init; } = [];

    /// <summary>
    /// The order of the data.
    /// Example: If there are 100 total records, with a Take of 10 records, then there will be 10
    /// possible values of Page, [0..9]. This helps display the records in order.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// The total number of records in the data source.
    /// </summary>
    public int Total { get; init; }

    /// <summary>
    /// The number of records filtered from the total.
    /// </summary>
    public int Filtered { get; init; }

    /// <summary>
    /// Optional: Error that occurs during processing.
    /// </summary>
    public Option<Error> Error { get; init; }
}