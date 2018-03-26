using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Astral.UI
{
    #region Event Declarations
    #region Delegates
    public delegate void SelectionWindowEventHandler(object sender, SelectionWindowEventArgs e);
    #endregion

    #region Class 'SelectionWindowEventArgs'
    public class SelectionWindowEventArgs : EventArgs
    {
        #region Class Members
        private ClosingReason m_reason;
        #endregion

        #region Constructors
        internal SelectionWindowEventArgs(ClosingReason reason)
        {
            m_reason = reason;
        }
        #endregion

        #region Properties
        public ClosingReason Reason
        {
            get { return m_reason; }
        }
        #endregion
    }
    #endregion
    #endregion

    #region Interface 'ISelectionWindow'
    interface ISelectionWindow
    {
        #region Events
        event SelectionWindowEventHandler SelectionWindowClosed;
        #endregion
    }
    #endregion
}
