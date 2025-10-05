namespace Foreflight.Components.Services;

class CsvWriter{
    private string filePath;

    public CsvWriter(string file){
        filePath = file;
    }

    public void WriteDataToCSV(List<(double x, double y)> data){
        using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using StreamWriter sw = new(fs);
        foreach (var item in data)
        {
            sw.WriteLine($"{item.x},{item.y}");
        }
    }
}