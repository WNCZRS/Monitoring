using System;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows;
using LiveCharts.Defaults;

namespace WpfTestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SeriesCollection SeriesCollection { get; set; }
        //public string[] Labels { get; set; }
        public Func<double, string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            //LineSeries LineSeries = new LineSeries();
            //SeriesCollection = new SeriesCollection();
            //SeriesCollection.Add(LineSeries);

            //LineSeries.Title = "Series 1";
            //LineSeries.Values = GetValuesForLineSeries();
            ////LineSeries.PointGeometry = null;

            ////{
            ////    new LineSeries
            ////    {
            ////        Title = "Series 1",
            ////        Values = new ChartValues<ObservablePoint>()
            ////        {
            ////            new ObservablePoint(1, 25),
            ////            new ObservablePoint(3, 98),
            ////            new ObservablePoint(5, 4),
            ////            new ObservablePoint(7, 54),
            ////            new ObservablePoint(9, 31)
            ////        },
            ////        PointGeometrySize = 0,
            ////        StrokeThickness = 4,
            ////        Fill = Brushes.Transparent

            ////    },
            ////    //new LineSeries
            ////    //{
            ////    //    Title = "Series 2",
            ////    //    Values = new ChartValues<double> { 34, 7, 35, 75 ,6 },
            ////    //    LineSmoothness = 0,
            ////    //    PointGeometry = null
            ////    //},
            ////    //new LineSeries
            ////    //{
            ////    //    Title = "Series 3",
            ////    //    Values = new ChartValues<double> { 4,78,7,25,87 },
            ////    //    LineSmoothness = 0,
            ////    //    PointGeometry = null
            ////    //}
            ////};

            ////Labels = new[] { "12:00", "13:00", "14:00", "15:00", "16:00" };
            ////YFormatter = value => value.ToString("C");
            ////Labels = value => new System.DateTime((long)(value * TimeSpan.FromMinutes(1).Ticks)).ToString("t");

            //Labels = value => new DateTime((long)((TimeSpan.FromMinutes(5).Ticks - (TimeSpan.FromSeconds(5).Ticks * value))/*/(10000*1000*60)*/)).ToString("t");

            ////modifying the series collection will animate and update the chart
            ////SeriesCollection.Add(new LineSeries
            ////{
            ////    Title = "Series 4",
            ////    Values = new ChartValues<double> { 5, 3, 2, 4 },
            ////    LineSmoothness = 0, //0: straight lines, 1: really smooth lines
            ////    PointGeometry = Geometry.Parse("m 25 70.36218 20 -28 -20 22 -8 -6 z"),
            ////    PointGeometrySize = 50,
            ////    PointForeround = Brushes.Gray
            ////});

            ////modifying any series values will also animate and update the chart
            ////SeriesCollection[3].Values.Add(5d);

            //DataContext = this;
        }

        private IChartValues GetValuesForLineSeries()
        {
            ChartValues<ObservablePoint> chartValues = new ChartValues<ObservablePoint>();
            for (int i = 0; i < 50; i++)
            {
                Random rand = new Random();
                int randInt = rand.Next(0, 100);
                chartValues.Add(new ObservablePoint(i, randInt));
            }                                                        
            return chartValues;
        }
    }
}
