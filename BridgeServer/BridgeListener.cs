using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace BridgeServer
{
	delegate void SetMessageDelegate(string msg);
	delegate void SetConnectionMessageDelegate(DateTime dt, bool local, int id, string msg);
	delegate void SetConnectionMessageByteArrayDelegate(DateTime dt, bool local, int id, byte[] msg, int msgLength);

    /// <summary>
    /// Server di ascolto
    /// </summary>
	class BridgeListener : IBridgeListener, IBridgeConnection
    {
        #region field
        private Thread _mainThread;
        private List<BridgeConnectionManager> _bcmList;
        private TcpListener _tcpListener = null;
		private BridgeConnectionDataConverter _connConverter;
		private IPEndPoint _remoteIP;
		private int _connectionCounter = 0;
        #endregion

		#region  prop
		public string LocalServer { get; private set; }
		public int LocalPort { get; set; }
		public string RemoteServer { get; set; }
		public int RemotePort { get; set; }

		public bool ConvertAddress { get; set; }
		#endregion

        #region ctor
        public BridgeListener()
        {
            LocalServer = "127.0.0.1";
            _bcmList = new List<BridgeConnectionManager>();
        }
        #endregion

		#region IBridgeConnection
		public bool Running { get; private set; }

		public void Start()
        {
            try
            {
				_remoteIP = new IPEndPoint(IPAddress.Parse(RemoteServer), RemotePort);
            }
            catch (Exception) { }
			if (_remoteIP == null)
            {
                try
                {
					_remoteIP = new IPEndPoint(Dns.GetHostAddresses(RemoteServer)[0], RemotePort);
                }
                catch (Exception) { }
            }

			if (_remoteIP == null)
            {
                Stop();
                return;
            }

			if (ConvertAddress) _connConverter = new BridgeConnectionDataConverter(LocalServer, LocalPort, RemoteServer, RemotePort);
			else _connConverter = null;

            try
            {
                //Attivazione server
                //tcpListener = new TcpListener(IPAddress.Parse(LocalServer), LocalPort);
				_tcpListener = new TcpListener(IPAddress.Any, LocalPort);
				_tcpListener.Start();

                //Attivazione del thread principale di ascolto
                Running = true;

                _mainThread = new Thread(this.ListenLoop);
                _mainThread.IsBackground = true;
                _mainThread.Start();

				if (OnMessage != null) OnMessage("Start listening on port " + ((IPEndPoint)_tcpListener.LocalEndpoint).Port.ToString());
            }
            catch (Exception)
            {
                Stop();
            }
        }

        public void Stop()
        {
            Running = false;

            try
            {
                if (_tcpListener != null) _tcpListener.Stop();
            }
            catch (Exception) { }

            if (_bcmList != null)
                foreach (BridgeConnectionManager btc in _bcmList)
                    btc.Stop();

            try
            {
                _mainThread.Abort();
            }
            catch (Exception) { }

			if (OnMessage != null) OnMessage("Stopped");
            UpdateConnectionStatus();
        }

		#endregion

		#region IBridgeListener
		public SetMessageDelegate OnMessage { get; set; }
		public SetMessageDelegate OnConnectionNumberChange { get; set; }
		public SetConnectionMessageDelegate OnConnectionMessage { get; set; }
		public SetConnectionMessageByteArrayDelegate OnConnectionMessageByteArray { get; set; }

		public void UpdateConnectionStatus()
		{
			if (OnConnectionNumberChange == null) return;

			int count = _bcmList.Count(item => item.Running);
			OnConnectionNumberChange(count + " / " + _bcmList.Count);
		}
		#endregion

		//Loop di ascolto
		private void ListenLoop()
		{
			try
			{
				//main bridge loop
				while (Running)
				{
					//--- in attesa di una connessione dal client
					TcpClient client = _tcpListener.AcceptTcpClient();
					_connectionCounter++;

					//--- gestione della connessione arrivata
					BridgeConnectionManager btc = new BridgeConnectionManager(_connectionCounter, client, _remoteIP, this, _connConverter);
					btc.Start();
					_bcmList.Add(btc);
					UpdateConnectionStatus();
				}
			}
			catch (Exception ex)
			{
				if (Running)
				{
					if (OnMessage != null) OnMessage("ListenLoop Exception: " + ex.ToString());
					Stop();
				}
			}
		}
	}
}
