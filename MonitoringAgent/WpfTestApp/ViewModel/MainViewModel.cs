using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using WpfTestApp.Model;
using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using System.Collections.Generic;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Data.SQLite;
using System.Data;
using Newtonsoft.Json;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace WpfTestApp.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private ObservableCollection<ClientOutput> clientOutputs;
        private List<ClientOutput> listCO;
        private SeriesCollection _seriesCollection;

        public SeriesCollection SeriesCollection
        {
            get
            {
                return _seriesCollection;
            }
            set
            {
                try
                {
                    _seriesCollection = value;
                    RaisePropertyChanged(() => SeriesCollection);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
        //public string[] Labels { get; set; }
        //public Func<double, string> Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

            listCO = new List<ClientOutput>();
            listCO.Add(ClientOutput.GetSampleClientOutput());

            LineSeries LineSeries = new LineSeries();
            LineSeries.Title = "Series 1";
            LineSeries.Values = GetValuesForLineSeries();

            SeriesCollection = new SeriesCollection();
            SeriesCollection.Add(LineSeries);
                 



            Thread graphThread = new Thread(new ThreadStart(GraphValuesController));
            try
            {
                graphThread.SetApartmentState(ApartmentState.STA);
                graphThread.IsBackground = true;
                //graphThread = new Thread(new ThreadStart(GraphValuesController));
                graphThread.Start();
            }
            catch (Exception ex)
            {
                graphThread.Abort();
                throw ex;
            } 


            LoadPluginOutputsCommand = new RelayCommand(LoadPluginOutputsMethod);

            //Labels = value => new DateTime((long)((TimeSpan.FromMinutes(5).Ticks - (TimeSpan.FromSeconds(5).Ticks * value))/*/(10000*1000*60)*/)).ToString("t");
        }

        private void GraphValuesController()
        {
            DateTime lastThreadTime = DateTime.MinValue;
            while (true)
            {
                if ((DateTime.Now - lastThreadTime).TotalSeconds >= 20)
                {
                    RefreshGraph();
                    lastThreadTime = DateTime.Now;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void RefreshGraph()
        {
            //LineSeries LineSeries = new LineSeries();
            //LineSeries.Title = "Series 1";
            //LineSeries.Values = GetValuesForLineSeries();

            //SeriesCollection = new SeriesCollection();
            //SeriesCollection.Add(LineSeries);


            /*Dispatcher.CurrentDispatcher.Invoke((Action)(() =>  
            SeriesCollection[0] = new LineSeries()
            {
                Title = "Series 1",
                Values = GetValuesForLineSeries()
            }));   */


            try
            {
                LineSeries ls = new LineSeries();
                ls.Title = "Series 1";
                ls.Values = GetValuesForLineSeries();
                SeriesCollection sc = new SeriesCollection();
                sc.Add(ls);

                Application.Current.Dispatcher.BeginInvoke(
                    DispatcherPriority.Background,
                    new Action(() => SeriesCollection = sc)
                );
            }
            catch (Exception ex)
            {
                throw;
            }





            //LineSeries ls = new LineSeries();
            //ls.Title = "Series 1";
            //ls.Values = GetValuesForLineSeries();
            ////SeriesCollection = new SeriesCollection();

            //SeriesCollection[0] = ls;

        }

        public ICommand LoadPluginOutputsCommand { get; private set; }

        public ObservableCollection<ClientOutput> ClientOutputList
        {
            get
            {
                return clientOutputs;
            }
        }
       
        public List<ClientOutput> ListCO
        {
            get
            {
                return listCO;
            }
        }

        public void LoadPluginOutputsMethod()
        {
            clientOutputs = new ObservableCollection<ClientOutput>();
            ClientOutput co = ClientOutput.GetSampleClientOutput();
            clientOutputs.Add(co);

            LineSeries LineSeries = new LineSeries();
            LineSeries.Title = "Series 1";
            LineSeries.Values = GetValuesForLineSeries();

            SeriesCollection = new SeriesCollection();
            SeriesCollection.Add(LineSeries);

            /*LineSeries LineSeries = new LineSeries();
            LineSeries.Title = "Series 1";
            LineSeries.Values = GetValuesForLineSeries(); */
            //if (SeriesCollection != null && SeriesCollection.Count > 0)
            //{
            //    SeriesCollection[0] = new LineSeries()
            //    {
            //        Title = "Series 1",
            //        Values = GetValuesForLineSeries()
            //    };
            //}
            //else
            //{
            //    SeriesCollection = new SeriesCollection();
            //    SeriesCollection.Add(
            //    new LineSeries()
            //    {
            //        Title = "Series 1",
            //        Values = GetValuesForLineSeries()
            //    });
            //}

            //Messenger.Default.Send<NotificationMessage>(new NotificationMessage("Plugin Outputs loaded."));
        }

        private IChartValues GetValuesForLineSeries()
        {
            string customer = "CustomerTest";
            string computerID = "C8CBB84172E0";
            int valuesCount = 50;

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

            int i = 0;
            ChartValues<ObservablePoint> chartValues = new ChartValues<ObservablePoint>();

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

                            chartValues.Add(new ObservablePoint(i, valueY));
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex )
            {
  
            }

            return chartValues;
        }
    }
}