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

        data.Add((0, 0));
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

    /*
        airplane does not descend
        the airplane takes off at 0,0

        the flight path is divided into 3 segments: first is climb between 2% and 20%, and has a X distance of 1000 feet or more.
        second is acceleration: the airplane does not climb but simply continues, has a segment of 1000 feet or more
        third is climb between 2% and 20% and conitnues until x-value of the last data point

        the gradients and lenghts of each segment are independent of each other

        hint: allowed to not only remove points but create new points. Can be helpful to allow for the merge of two points into one -> still
        needs to be conservative and safe. Also needs to be good, measured by the amount of possible flight paths that are restricted.

        Challenge: Create algorithm that filters all the obstacles conservatively and is not allowed to output more than 10 points in each file.
        Try to minimize the number of possible flight paths it restricts.

    */
    public void ThreeSegmentFilter()
    {
        double minClimb = 0.02;
        double maxClimb = 0.20;
        double minSegmentLength = 1000;

        if (dataPoints == null || dataPoints.Count == 2)
            return;

        // we assume list is sorted

        dataPoints.Insert(0, (0, 0)); // ensure starting point

        //segment 1
        double totalDist = dataPoints[^1].x; // find the furthest horizontal point we have
        if (totalDist < 2 * minSegmentLength)
        {

        }
        //segment 2


        //segment 3


    }


    /// <summary>
    /// Sadly flawed
    /// </summary>
    public void ClimbRateFilterWithTakeOffDist(double takeOffDist)
    {
        if (dataPoints.Count == 0) return;

        StepFilter();

        double takeOffStart = -takeOffDist;
        double takeOffEnd = 0;

        var validPoints = new List<(double x, double y)>();
        var lines = new List<(double a, double b)>();


        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            var (x1, y1) = dataPoints[i];
            var (x2, y2) = dataPoints[i + 1];

            double a = -((y1 - y2) / (x1 - x2));
            double b = -(y1 - a * x1);

            lines.Add((a, b));
        }

        for (int i = 0; i < dataPoints.Count; i++)
        {
            var (x1, y1) = dataPoints[i];
            bool keep = false;

            for (int j = 0; j < dataPoints.Count; j++)
            {
                if (i == j) continue;
                var (x2, y2) = dataPoints[j];
                if (x1 == x2) continue;

                double m = ((y1 - y2) / (x1 - x2));
                double b = (y1 - m * x1);

                /*foreach (var line in lines)
                {
                    double x0 = (-1 * b - (-1) * line.b) / (m * (-1) - line.a * (-1));
                    double y0 = (line.b * m - b * line.a) / (line.a * (-1) - m * (-1));
                }*/

                double xCross = -b / m;

                if (xCross > takeOffStart && xCross <= takeOffEnd)
                {
                    keep = true;
                    break;
                }
            }

            if (keep)
                validPoints.Add((x1, y1));
        }

        dataPoints = validPoints;
    }

}