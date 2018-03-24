using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace TestingConcepts
{
    /// <summary>
    /// Interaction logic for EasingSelector.xaml
    /// </summary>
    public partial class EasingSelector : UserControl, INotifyPropertyChanged
    {
        #region Instance Variables

        private EasingType type = EasingType.Linear;

        #endregion

        #region Properties

        public EasingType EasingType
        {
            get
            {
                return this.type;
            }
            private set
            {
                this.type = value;
                RaisePropertyChanged("EasingType");
            }
        }

        #endregion

        #region Property Changed Event

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        #region Constructor with Inline Events

        public EasingSelector()
        {
            InitializeComponent();

            this.SineInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.SineEaseIn;
            this.SineOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.SineEaseOut;
            this.SineInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.SineEaseInOut;

            this.CubicInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.CubicEaseIn;
            this.CubicOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.CubicEaseOut;
            this.CubicInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.CubicEaseInOut;

            this.QuinticInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuinticEaseIn;
            this.QuinticOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuinticEaseOut;
            this.QuinticInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuinticEaseInOut;

            this.CircleInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.CircularEaseIn;
            this.CircleOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.CircularEaseOut;
            this.CircleInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.CircularEaseInOut;
            
            this.ElasticInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.ElasticEaseIn;
            this.ElasticOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.ElasticEaseOut;
            this.ElasticInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.ElasticEaseInOut;


            this.QuadraticInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuadraticEaseIn;
            this.QuadraticOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuadraticEaseOut;
            this.QuadraticInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuadraticEaseInOut;
            
            this.QuarticInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuarticEaseIn;
            this.QuarticOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuarticEaseOut;
            this.QuarticInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.QuarticEaseInOut;

            this.ExpInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.ExponentialEaseIn;
            this.ExpOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.ExponentialEaseOut;
            this.ExpInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.ExponentialEaseInOut;

            this.BackInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.BackEaseIn;
            this.BackOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.BackEaseOut;
            this.BackInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.BackEaseInOut;

            this.BounceInCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.BounceEaseIn;
            this.BounceOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.BounceEaseOut;
            this.BounceInOutCanvas.MouseLeftButtonUp += (s, e) => this.EasingType = EasingType.BounceEaseInOut;

            this.LinearButton.Click += (s, e) => this.EasingType = EasingType.Linear;
            
        }

        #endregion
    }
}
