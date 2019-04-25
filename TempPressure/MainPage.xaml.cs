using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Uwp;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TempPressure
{
    /// <summary>
    /// Control Screen for reading and displaying temperature data
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BMP280 _bmp280;
        private ActianZenDataSource _ds;
        private string _deviceName; 
        public ObservableCollection<Reading> Readings { get; set; } 

        //chart items
        public SeriesCollection Series { get; set; }
        public Func<double, string> LabelFormatter { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
           
        }

        //This method will be called by the application framework when the page is first loaded
        protected override async void OnNavigatedTo(NavigationEventArgs navArgs)
        {
            try
            {

                //initialize public properties
                //Create a new object for our barometric sensor class
                _bmp280 = new BMP280();
                //Initialize the sensor
                await _bmp280.Initialize();

                _ds = new ActianZenDataSource();
                _deviceName = "CPI3";
                Readings = new ObservableCollection<Reading>();
                                              
                //initialize series configuration and label formatting for the chart
                var chartConfig = Mappers.Xy<Reading>()
                    .X(model => (double)model.ReadingTs.Ticks)
                    .Y(model => model.Temperature);
                Series = new SeriesCollection(chartConfig);
                LabelFormatter = value => new System.DateTime((long)value).ToString("hh:mm:ss tt");

                this.DataContext = this;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private async void BtnCollectReadings_Click(object sender, RoutedEventArgs e)
        {
                try
                {

                    //Create variables to store the sensor data: temperature, pressure and altitude. 
                    //Initialize them to 0.
                    double temp = 0;
                    double pressure = 0;
                    double altitude = 0;

                    //Create a constant for pressure at sea level. 
                    //This is based on your local sea level pressure (Unit: Hectopascal)
                    //visit https://www.weather.gov/ and input zip code to obtain the barometer value
                    const double seaLevelPressure = 1013.5;

                    //Read 10 samples of the data at an interval of 1 second
                    for (int i = 0; i < 10; i++)
                    {
                        temp = await _bmp280.ReadTemperature();
                        pressure = await _bmp280.ReadPressure();
                        altitude = await _bmp280.ReadAltitude(seaLevelPressure);

                        //Write the values to your debug console
                        Debug.WriteLine("Temperature: " + temp.ToString() + " deg C");
                        Debug.WriteLine("Pressure: " + pressure.ToString() + " Pa");
                        Debug.WriteLine("Altitude: " + altitude.ToString() + " m");

                        //add the reading to the Actian table
                        _ds.AddReading(new Reading()
                        {
                            DeviceName = _deviceName,
                            Temperature = temp,
                            Pressure = pressure,
                            Altitude = altitude,
                            ReadingTs = DateTime.Now
                        });

                        await Task.Delay(1000);
                    }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void BtnRefreshReadings_Click(object sender, RoutedEventArgs e)
        {

            //update list and chart with values retrieved from the Actian database

            Readings.Clear();
            Series.Clear();

            var chartValues = new ChartValues<Reading>();
            var readings =  _ds.GetReadings();
            readings.ForEach((read) => {
                Readings.Add(read);
                chartValues.Add(read);
            });
                        
            Series.Add(new LineSeries() { Values = chartValues });
        }

        private void BtnDropTable_Click(object sender, RoutedEventArgs e)
        {
            //clean up - remove readings table from the database
            _ds.DropTable();
        }
    }
    
}
