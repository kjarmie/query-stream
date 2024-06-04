namespace QueryStream.Tests;

public class EnumerableExtensionsTests
{
    [Theory]
    [InlineData(2, 10)]
    public void ToDataQueryResponse_PagedData_ExpectNResultsPerPage(int page, int numPerPage)
    {
        var enumerable = Enumerable.Range(0, 100);
        // var dataQuery = new DataQuery();
        //
        // var result = enumerable.ToDataQueryResult(dataQuery);
    }
}