using BlazorBootstrap;

namespace Foreflight.Components.Services;

public class CsvService
{
    private string filePath;

    public CsvService(string file)
    {
        filePath = file;
    }

    /// <summary>
    /// Parses a CSV file with two columns of doubles into a list of ScatterChartDataPoint.
    /// </summary>
    public List<ScatterChartDataPoint?> CsvScatterChartParser()
    {
        var dataPoints = CsvParser();

        var toReturn = new List<ScatterChartDataPoint?>();
        foreach (var point in dataPoints)
        {
            toReturn.Add(new(point.x, point.y));
        }
        return toReturn;
    }

    /// <summary>
    /// Parses a CSV file with two columns of doubles into a list of tuples.
    /// </summary>
    public List<(double x, double y)> CsvParser()
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File does not exist", filePath);
        }

        List<(double x, double y)> parsed = new();

        using (var reader = new StreamReader(filePath))
        {
            string? line;
            bool header = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (header)
                {
                    header = false;
                    continue; // Skip header line
                }
                
                string[] elements = line.Split(",");

                try
                {
                    if (elements.Length >= 2)
                    {
                        double x = double.Parse(elements[0].Trim());
                        double y = double.Parse(elements[1].Trim());
                        parsed.Add((x, y));
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException($"Could not parse CSV file, error on line: {line}", ex);
                }
            }
            return parsed;
        }
    }

}