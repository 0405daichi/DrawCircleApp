using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

namespace WpfApp4
{
    public class MyLine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private double _setRadius;
        private double _digree;
        private Size _radius;
        private bool isSelected;
        public MyLine()
        {
            Start = new LinePoint(100, 50, this, false);
            End = new LinePoint(50, 100, this, false);
            Center = new LinePoint(50, 50, this, false);
            _setRadius = 0;
            _radius.Width = 50;
            _radius.Height = 50;
            isSelected = false;
        }

        public LinePoint Start { get; set; }
        public LinePoint End { get; set; }
        public LinePoint Center { get; set; }
        public LinePoint CenterRangeStart { get; set; }
        public LinePoint CenterRangeEnd { get; set; }
        public string CircleName { get; set; }
        
        public double SetRadius
        {
            get { return _setRadius; }
            set
            {
                if (_setRadius == value)
                    return;
                _setRadius = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SetRadius"));
            }
        }

        public double Digree
        {
            get { return _digree; }
            set
            {
                if (_digree == value)
                    return;
                _digree = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Digree"));
            }
        }
        
        public Size Radius
        {
            get { return _radius; }
            set
            {
                if (_radius == value)
                    return;
                _radius = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Radius"));
            }
        }

        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                if (isSelected != value)
                {

                    isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
                }
            }
        }
    }
}
