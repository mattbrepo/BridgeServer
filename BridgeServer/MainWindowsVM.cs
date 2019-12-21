using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace BridgeServer
{
	/// <summary>
	/// View Model di MainWindow
	/// </summary>
	class MainWindowsVM : INotifyPropertyChanged
	{
		#region const
		private const string TITLE = "BridgeServer v0.5";
		#endregion

		#region enum
		enum EButtonLabel
		{
			START,
			STOP
		}
		#endregion

		#region field
		private BridgeListener _bl;
		#endregion

		#region prop
		private string _title;
		public string Title
		{
			get
			{
				return TITLE + ((_title == null || _title == "0 / 0") ? "" : " (" + _title + ")");
			}
			set
			{
				_title = value;
				NotifyPropertyChanged("Title");
			}
		}

		private StringBuilder _mainMsg = new StringBuilder();
		public string MainMsg
		{
			get
			{
				return _mainMsg.ToString();
			}
			set
			{
				if (value == "") return;

				if (value == null) _mainMsg = new StringBuilder();
				else _mainMsg.AppendLine(value);

				MsgNotifyPropertyChanged("MainMsg");
			}
		}

		private StringBuilder _localMsg = new StringBuilder();
		public string LocalMsg
		{
			get
			{
				return _localMsg.ToString();
			}
			set
			{
				if (value == "") return;

				if (value == null) _localMsg = new StringBuilder();
				else _localMsg.AppendLine(value);

				MsgNotifyPropertyChanged("LocalMsg");
			}
		}

		private StringBuilder _remoteMsg = new StringBuilder();
		public string RemoteMsg
		{
			get
			{
				return _remoteMsg.ToString();
			}
			set
			{
				if (value == "") return;

				if (value == null) _remoteMsg = new StringBuilder();
				else _remoteMsg.AppendLine(value);

				MsgNotifyPropertyChanged("RemoteMsg");
			}
		}

		private string _buttonLabel = EButtonLabel.START.ToString();
		public string ButtonLabel
		{
			get
			{
				return _buttonLabel;
			}
			set
			{
				_buttonLabel = value;
				NotifyPropertyChanged("ButtonLabel");
			}
		}
		#endregion

		#region func

		/// <summary>
		/// Cambio stato: start/stop
		/// </summary>
		public void ChangeStatus()
		{
			if (_bl != null && _bl.Running)
			{
				//stopping...
				ButtonLabel = EButtonLabel.START.ToString();
				_bl.Stop();
				return;
			}

			//starting...
			ButtonLabel = EButtonLabel.STOP.ToString();
			this.MainMsg = null;
			this.LocalMsg = null;
			this.RemoteMsg = null;

			_bl = new BridgeListener();

			//connection data
			_bl.LocalPort = Properties.Settings.Default.LocalPort;
			_bl.RemoteServer = Properties.Settings.Default.RemoteServer;
			_bl.RemotePort = Properties.Settings.Default.RemotePort;

			//options
			_bl.ConvertAddress = Properties.Settings.Default.ConvertAddress;

			//handler
			_bl.OnConnectionNumberChange = SetConnectionNumberChange;
			if (Properties.Settings.Default.Debug)
			{
				_bl.OnMessage = AddMessage;
				_bl.OnConnectionMessage = AddConnectionMessage;
				if (Properties.Settings.Default.DebugInByte) _bl.OnConnectionMessageByteArray = AddConnectionMessageByteArrayToString;
				else _bl.OnConnectionMessageByteArray = AddConnectionMessageByteArray;
			}

			_bl.Start();
		}

		/// <summary>
		/// Interrompe tutte il bridge server se necessario
		/// </summary>
		public void StopAll()
		{
			if (_bl != null) _bl.Stop();
		}

		/// <summary>
		/// Imposta il cambio di numero di connessioni
		/// </summary>
		/// <param name="msg"></param>
		private void SetConnectionNumberChange(string msg)
		{
			this.Title = msg;
		}

		/// <summary>
		/// Aggiunge messaggio generico
		/// </summary>
		/// <param name="msg"></param>
		private void AddMessage(string msg)
		{
			MainMsg = msg;
		}

		/// <summary>
		/// Aggiunge messaggio di una connessione
		/// </summary>
		/// <param name="local">proveniente da local o remote</param>
		/// <param name="id">id della connessione</param>
		/// <param name="msg">dati inviati da local o remote</param>
		private void AddConnectionMessage(DateTime dt, bool local, int id, string msg)
		{
			if (local)
			{
				_localMsg.AppendLine(CreatePreMessage(dt, id) + msg);
				MsgNotifyPropertyChanged("LocalMsg");
			}
			else
			{
				_remoteMsg.AppendLine(CreatePreMessage(dt, id) + msg);
				MsgNotifyPropertyChanged("RemoteMsg");
			}
		}

		/// <summary>
		/// Aggiunge messaggio (array di byte) di una connessione 
		/// </summary>
		/// <param name="local"></param>
		/// <param name="id"></param>
		/// <param name="msg"></param>
		/// <param name="msgLength"></param>
		private void AddConnectionMessageByteArray(DateTime dt, bool local, int id, byte[] msg, int msgLength)
		{
			string sMsg = BitConverter.ToString(msg, 0, msgLength);
			AddConnectionMessage(dt, local, id, sMsg);
		}

		/// <summary>
		/// Aggiunge messaggio (array di byte) di una connessione 
		/// convertendo l'array di byte in stringa...si assume pertanto che l'array contenga 
		/// caratteri stampabili
		/// </summary>
		/// <param name="local"></param>
		/// <param name="id"></param>
		/// <param name="msg"></param>
		/// <param name="msgLength"></param>
		private void AddConnectionMessageByteArrayToString(DateTime dt, bool local, int id, byte[] msg, int msgLength)
		{
			string sMsg = System.Text.Encoding.ASCII.GetString(msg, 0, msgLength);
			AddConnectionMessage(dt, local, id, sMsg);
		}

		private string CreatePreMessage(DateTime dt, int id)
		{
			//DateTime dt = DateTime.Now;
			return "[" + dt.ToString() + "." + dt.Millisecond + ", " + id + "] ";
		}
		#endregion

		#region INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void MsgNotifyPropertyChanged(string propertyName)
		{
			//non si notifica il messaggio se l'expander 'Verbose' è chiuso
			if (!BridgeServer.Properties.Settings.Default.Verbose) return;
			NotifyPropertyChanged(propertyName);
		}

		public void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public void NotifyPropertyChanged(object sender, string propertyName)
		{
			if (!BridgeServer.Properties.Settings.Default.Verbose) return;

			if (PropertyChanged != null) PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
