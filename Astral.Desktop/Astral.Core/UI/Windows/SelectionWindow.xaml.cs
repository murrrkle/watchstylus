using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

using Astral.Device;

namespace Astral.UI
{
    #region Partial Class 'SelectionWindow'
    public partial class SelectionWindow : Window
    {
        #region Static Class Members
        protected static readonly double HotCornerSize = 20.0;

        protected static readonly double MinimumSize = 40.0;
        #endregion

        #region Class Members
        private Display m_display;
        #endregion

        #region Events

        #endregion

        #region Constructors
        public SelectionWindow()
            : this(null)
        { }

        internal SelectionWindow(Display display)
        {
            m_display = display;
            InitializeComponent();
        }
        #endregion

        #region Event Handler
        #region Key Event Handler
        private void WindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }
        #endregion
        #endregion
    }
    #endregion
}
