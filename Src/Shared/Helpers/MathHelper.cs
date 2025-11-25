namespace Shared.Helpers;

public static class MathHelper
{
    /// <summary>
    /// محاسبه رگرسیون خطی ساده (y = mx + b) با روش حداقل مربعات
    /// </summary>
    public static (double Slope, double Intercept, double RSquared) CalculateLinearRegression(List<double> xValues, List<double> yValues)
    {
        if (xValues.Count != yValues.Count || xValues.Count < 2)
            return (0, 0, 0);

        int n = xValues.Count;
        double sumX = xValues.Sum();
        double sumY = yValues.Sum();
        double sumXY = xValues.Zip(yValues, (x, y) => x * y).Sum();
        double sumX2 = xValues.Sum(x => x * x);

        double denominator = (n * sumX2) - (sumX * sumX);

        // جلوگیری از تقسیم بر صفر (اگر همه Xها برابر باشند)
        if (Math.Abs(denominator) < 1e-10) return (0, 0, 0);

        double slope = ((n * sumXY) - (sumX * sumY)) / denominator;
        double intercept = (sumY - (slope * sumX)) / n;

        // محاسبه R-Squared
        double yMean = sumY / n;
        double ssTotal = yValues.Sum(y => Math.Pow(y - yMean, 2));
        double ssRes = xValues.Zip(yValues, (x, y) => Math.Pow(y - ((slope * x) + intercept), 2)).Sum();

        double rSquared = ssTotal == 0 ? 1 : 1 - (ssRes / ssTotal);

        return (slope, intercept, rSquared);
    }
}