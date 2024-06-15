namespace Mahsul.Models
{
    public class StatisticsViewModel
    {
        public List<SalesData> YearlySales { get; set; }
        public int ?TotalUsers { get; set; }
        public int ?TotalPurchases { get; set; }
    }

    public class SalesData
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public int TotalSales { get; set; }
        public string MonthName { get; set; }
    }
}
