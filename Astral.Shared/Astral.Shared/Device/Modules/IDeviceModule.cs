using System;
using System.Collections.Generic;
using System.Text;

using Astral.Messaging;
using Astral.Net;
using Astral.Net.Serialization;

namespace Astral.Device
{
    #region Abstract Class 'IDeviceModule'
    public abstract class IDeviceModule : ITransferable
    {
        #region Constant Class Members
        private const string ModuleNameField = "modN";

        internal const string ModuleHandlerField = "modH";
        #endregion

        #region Class Members
        private AstralDevice m_host;

        private string m_name;

        private bool m_active = false;

        private double m_accuracy = 0.0;
        #endregion

        #region Events
        public event EventHandler Activated;

        public event EventHandler Deactivated;
        #endregion

        #region Constructors
        protected IDeviceModule(string name)
        {
            m_name = name;
        }

        internal IDeviceModule(NetworkStreamInfo info)
        {
            m_name = info.GetString(ModuleNameField);
        }
        #endregion

        #region Properties
        public string Name
        {
            get { return m_name; }
        }

        public bool IsActive
        {
            get { return m_active; }
            private set
            {
                bool changed = (m_active != value);
                m_active = value;

                if (changed)
                {
                    SendStatusMessage(m_active);
                }
            }
        }

        internal AstralDevice Host
        {
            private get { return m_host; }
            set { m_host = value; }
        }

        public double Accuracy
        {
            get { return m_accuracy; }
            set
            {
                bool changed = (m_accuracy != value);
                m_accuracy = value;

                if (changed)
                {
                    SendAccuracyMessage(m_accuracy);
                }
            }
        }

        public abstract ModuleType Type { get; }
        #endregion

        #region Accuracy
        private void SendAccuracyMessage(double accuracy)
        {
            Message msg = ModuleAccuracyMessage.CreateInstance(accuracy);
            SendMessage(msg);
        }
        #endregion

        #region Activation/Deactivation
        private void SendStatusMessage(bool enableModule)
        {
            Message msg = ModuleStatusMessage.CreateInstance(enableModule);
            SendMessage(msg);
        }

        protected void Activate()
        {
            IsActive = true;
        }

        protected void Deactive()
        {
            IsActive = false;
        }
        #endregion

        #region Messaging
        internal void SendMessage(Message msg)
        {
            if (!(msg.ContainsField(ModuleHandlerField)))
            {
                msg.AddField(ModuleHandlerField, (int)Type);
            }

            if (Host != null)
            {
                Host.SendMessage(msg);
            }
        }

        internal void HandleModuleMessage(Message msg)
        {
            if (ModuleStatusMessage.IsKindOf(msg))
            {
                // handle it here
                bool activate = ModuleStatusMessage.ToStatus(msg);

                bool changed = (m_active != activate);
                m_active = activate;

                if (changed)
                {
                    if (m_active)
                    {
                        Activated?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        Deactivated?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            else if (ModuleAccuracyMessage.IsKindOf(msg))
            {
                // handle it here
                m_accuracy = ModuleAccuracyMessage.ToAccuracy(msg);
            }
            else
            {
                ProcessModuleMessage(msg);
            }
        }

        protected abstract void ProcessModuleMessage(Message msg);
        #endregion

        #region Transferrable
        internal abstract void GetStreamDataInternal(NetworkStreamInfo info);
        #endregion

        #region Overrides (ITransferable)
        public void GetStreamData(NetworkStreamInfo info)
        {
            info.AddValue(ModuleNameField, m_name);
            GetStreamDataInternal(info);
        }
        #endregion
    }
#endregion
}
