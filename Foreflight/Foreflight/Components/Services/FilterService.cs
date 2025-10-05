using BlazorBootstrap;

namespace Foreflight.Components.Services;

class FilterService
{
    public List<(double x, double y)> dataPoints {get; private set;}

    public FilterService(List<(double x, double y)> dataPoints){
        this.dataPoints = dataPoints;
    }

    /// <summary>
    /// Find the fewest points by always choosing the farthest point that doesnt violate tthe constant climb rate.
    /// </summary>
    public void ClimbRateFilter()
    {
        /*
            We never go down in height (y value) as we move along the x axis.
            We start at (0, 0)
            We have constant climb rate. 
            If the plane would fail to climb in the original data it should fail to climb in the filtered data
        */
        if (dataPoints.Count <= 2) return;

        List<(double x, double y)> data = new(); ;

        // while a condition is true
        double largest = 0;
        double pointX = 0;
        double pointY = 0;
        for (int i = 0; i < dataPoints.Count; i++)
        {
            double farthestValidValue = dataPoints[i].y;
            double a = (dataPoints[i].y) / (dataPoints[i].x);
            if (a > largest)
            {
                largest = a;
                pointX = dataPoints[i].x;
                pointY = dataPoints[i].y;
            }
        }

        data.Add(0,0);
        data.Add((pointX, pointY));
        dataPoints = data;
    }

    /// <summary>
    /// Removes any point that is lower than or at the same height as the previous point.
    /// </summary>
    public void StepFilter()
    {
        double top = 0;

        List<(double x, double y)> newData = new();
        foreach (var item in dataPoints)
        {
            if (item.y >= top)
            {
                top = item.y;
                newData.Add(item);
                continue;
            }
        }

        dataPoints = newData;
    }

}