namespace RconnectAPI.Models;

public class ResponseData<T>
{
    public List<T> data;
    public long? totalCount;

    public ResponseData(List<T> data, long? totalCount)
    {
        this.data = data;
        this.totalCount = totalCount;
    }

    public long? TotalCount
    {
        get => totalCount;
        set => totalCount = value;
    }

    public List<T> Data
    {
        get => data;
        set => data = value ?? throw new ArgumentNullException(nameof(value));
    }
}