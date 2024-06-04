namespace Application.Utilities.DataSource;

/// <summary>Specifies the direction of a sort operation.</summary>
public enum SortDirection
{
    /// <summary>Sorts in ascending order.</summary>
    Ascending,
    /// <summary>Sorts in descending order.</summary>
    Descending,
}


public class SortDescriptor
{
    public SortDescriptor(string columnName, SortDirection order)
    {
        this.ColumnName = columnName;
        this.SortDirection = order;
    }

    public string ColumnName { get; set; }

    public SortDirection SortDirection { get; set; }
}