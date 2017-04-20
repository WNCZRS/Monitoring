using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Configurations;
using WpfApplication1.Model;
using LiveCharts.Defaults;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;

namespace Wpf.CartesianChart.ConstantChanges
{
    public partial class ConstantChangesChart : UserControl, INotifyPropertyChanged
    {
        private double _axisMax;
        private double _axisMin;

        public ConstantChangesChart()
        {
            InitializeComponent();

            //To handle live data easily, in this case we built a specialized type
            //the MeasureModel class, it only contains 2 properties
            //DateTime and Value
            //We need to configure LiveCharts to handle MeasureModel class
            //The next code configures MeasureModel  globally, this means
            //that LiveCharts learns to plot MeasureModel and will use this config every time
            //a IChartValues instance uses this type.
            //this code ideally should only run once
            //you can configure series in many ways, learn more at 
            //http://lvcharts.net/App/examples/v1/wpf/Types%20and%20Configuration

            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            //the values property will store our values array
            ChartValues = new ChartValues<MeasureModel>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("hh:mm:ss");

            //AxisStep forces the distance between each separator in the X axis
            AxisStep = TimeSpan.FromSeconds(5).Ticks;
            //AxisUnit forces lets the axis know that we are plotting seconds
            //this is not always necessary, but it can prevent wrong labeling
            AxisUnit = TimeSpan.TicksPerSecond * 5;

            SetAxisLimits(DateTime.Now);

            //The next code simulates data changes every 300 ms

            IsReading = false;

            DataContext = this;
        }

        public ChartValues<MeasureModel> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        public double AxisMax
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }
        public double AxisMin
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }

        public bool IsReading { get; set; }

        private void Read()
        {
            var r = new Random();

            while (IsReading)
            {
                Thread.Sleep(1500);
                var now = DateTime.Now;

                ChartValues.Add(GetValuesForLineSeries());

                SetAxisLimits(now);

                //lets only use the last 150 values
                if (ChartValues.Count > 150) ChartValues.RemoveAt(0);
            }
        }

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
            AxisMin = now.Ticks - TimeSpan.FromSeconds(120).Ticks; // and 8 seconds behind
        }

        private void InjectStopOnClick(object sender, RoutedEventArgs e)
        {
            IsReading = !IsReading;
            if (IsReading) Task.Factory.StartNew(Read);
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        private MeasureModel GetValuesForLineSeries()
        {
            string customer = "CustomerTest";
            string computerID = "BFEBFBFF000306A9";
            int valuesCount = 1;

            List<PluginOutputCollection> listOfPOC = new List<PluginOutputCollection>();
            List<ClientOutput> clientOutputList = new List<ClientOutput>();
            string connectionString = string.Format("Data Source=d:\\Monitoring\\MonitoringServerDB.sqlite; Version=3;");

            try
            {
                using (SQLiteConnection dbConnection = new SQLiteConnection(connectionString))
                {
                    dbConnection.Open();
                    SQLiteCommand cmd = new SQLiteCommand(@"SELECT RecID, recCreated, ComputerName, JSON FROM MonitoringServerStorage 
                                                            WHERE Customer = @Customer 
                                                            and ComputerID = @ComputerID 
                                                            ORDER BY RecCreated desc LIMIT @ValuesCount", dbConnection);
                    cmd.Parameters.AddWithValue("@Customer", customer);
                    cmd.Parameters.AddWithValue("@ComputerID", computerID);
                    cmd.Parameters.AddWithValue("@ValuesCount", valuesCount);

                    SQLiteDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    while (reader.Read())
                    {
                        ClientOutput co = new ClientOutput(reader.GetString(2), computerID, customer);
                        co.CollectionList = JsonConvert.DeserializeObject<List<PluginOutputCollection>>(reader.GetString(3));
                        clientOutputList.Add(co);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            MeasureModel measureModel = new MeasureModel();

            try
            {
                clientOutputList.Reverse();
                foreach (ClientOutput co in clientOutputList)
                {
                    foreach (PluginOutputCollection poc in co.CollectionList)
                    {
                        if (poc.PluginName == "Performance")
                        {
                            double valueY = Convert.ToDouble(poc.PluginOutputList[0].Values[0].Value.ToString().Split(' ')[0]);

                            measureModel.DateTime = DateTime.Now;
                            measureModel.Value = valueY;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return measureModel;
        }
    }
}