namespace RconnectAPI.Models;

public class ListResponseData<T>
{
    private List<T> data;

    public ListResponseData(List<T> data, long? totalCount = 1)
    {
        this.Data = data;
        this.TotalCount = totalCount;
    }

    public long? TotalCount { get; set; }

    public List<T> Data
    {
        get => data;
        set => data = value ?? throw new ArgumentNullException(nameof(value));
    }
}
public class ResponseData<T>(T data)
{
    public T data = data;

    public T Data
    {
        get => data;
        set => data = value ?? throw new ArgumentNullException(nameof(value));
    }
}