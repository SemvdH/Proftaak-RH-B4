using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiveCharts;
using LiveCharts.Configurations;
using Util;

namespace DoctorApp.Models
{
    class Chart : ObservableObject,INotifyPropertyChanged
    {

        private double _axisMax;
        private double _axisMin;
        private double _trend;

        public event PropertyChangedEventHandler PropertyChanged;

        public ChartValues<MeasureModel> ChartValues { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }

        public PatientInfo PatientInfo { get; set; }
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




        public Chart(PatientInfo patientInfo)
        {
            var mapper = Mappers.Xy<MeasureModel>()
            .X(model => model.DateTime.Ticks)
            .Y(model => model.Value);

            Charting.For<MeasureModel>(mapper);

            ChartValues = new ChartValues<MeasureModel>();

            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            AxisStep = TimeSpan.FromSeconds(1).Ticks;

            AxisUnit = TimeSpan.TicksPerSecond;

            SetAxisLimits(DateTime.Now);

            IsReading = true;

            ChartValues.Add(new MeasureModel
            {
                DateTime = DateTime.Now,
                Value = 8
            });

        }

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
            AxisMin = now.Ticks - TimeSpan.FromSeconds(8).Ticks; // and 8 seconds behind
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        
        public void NewValue(double value)
        {
            var now = DateTime.Now;
            _trend = value;
            ChartValues.Add(new MeasureModel
            {
                DateTime = now,
                Value = _trend
            });

            SetAxisLimits(now);

            if (ChartValues.Count > 150) ChartValues.RemoveAt(0);
        }

        public void Clear()
        {
            Debug.WriteLine("clear");
            ChartValues.Clear();
        }



    }

    public class MeasureModel
    {
        public DateTime DateTime { get; set; }
        public double Value { get; set; }
    }
}
