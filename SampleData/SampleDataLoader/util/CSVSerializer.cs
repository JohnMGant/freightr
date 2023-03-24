using System;
using System.Reflection;

public class CSVSerializer
{
    public static void Serialize<T>(Stream stream, IEnumerable<T> items, bool includeHeaderRow = false, bool forceQuoteStrings = false)
    {
        using CSVSerializer<T> serializer = new(stream, includeHeaderRow, forceQuoteStrings);
        serializer.Serialize(items);
    }
}

public class CSVSerializer<T> : IDisposable
{
    private readonly StreamWriter writer;
    private readonly bool includeHeaderRow;
    private readonly bool forceQuoteStrings;
    private List<string> lineValues;
    private readonly IEnumerable<PropertyInfo> properties;

    public CSVSerializer(Stream stream, bool includeHeaderRow = false, bool forceQuoteStrings = false)
    {
        this.writer = new(stream);
        this.includeHeaderRow = includeHeaderRow;
        this.forceQuoteStrings = forceQuoteStrings;
        lineValues = new();
        properties = typeof(T).GetProperties();
    }

    public void Dispose()
    {
        writer.Dispose();
    }

    public void Serialize(IEnumerable<T> items)
    {
        if (includeHeaderRow)
        {
            BuildHeaderLine();
            FlushLine();
        }
        foreach (T item in items)
        {
            BuildRecordLine(item);
            FlushLine();
        }
        writer.Flush();
    }

    private void BuildHeaderLine()
    {
        lineValues.AddRange(properties.Select(p => p.Name));
    }

    private void BuildRecordLine(T item)
    {
        foreach (PropertyInfo property in properties)
        {
            if (property.PropertyType == typeof(string))
            {
                string? value = (string?)property.GetValue(item);
                if (value is null)
                {
                    lineValues.Add(string.Empty);
                }
                else if (value.Contains(',') || forceQuoteStrings)
                {
                    lineValues.Add($"\"{value}\"");
                }
                else
                {
                    lineValues.Add(value);
                }
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                DateTime? value = (DateTime?)property.GetValue(item);
                if (value is null)
                {
                    lineValues.Add(string.Empty);
                }
                else
                {
                    lineValues.Add($"\"{value.Value:R}\"");
                }
            }
            else
            {
                object? value = property.GetValue(item);
                if (value is null)
                {
                    lineValues.Add(string.Empty);
                }
                else
                {
                    lineValues.Add(value.ToString()!);
                }
            }
        }
    }

    private void FlushLine()
    {
        writer.WriteLine(string.Join(',', lineValues));
        lineValues.Clear();
    }

}

