using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace BridgeServer
{
    /// <summary>
    /// Gestore delle due connessioni (sender/receiver) tra il client e il server
    /// </summary>
    class BridgeConnectionManager : IBridgeConnection
    {
        #region field
        private BridgeConnection _bcLocal;
        private BridgeConnection _bcRemote;
        #endregion

        #region ctor
		public BridgeConnectionManager(int idConnection, TcpClient local, IPEndPoint remoteIP, IBridgeListener bl, BridgeConnectionDataConverter connConverter)
        {
            TcpClient remote = null;
            try
            {
                remote = new TcpClient();
                remote.Connect(remoteIP);
            }
            catch (Exception)
            {
				if (bl.OnConnectionMessage != null) bl.OnConnectionMessage(DateTime.Now, false, idConnection, "Connection impossible on remote");
            }

            if (remote == null) return;

			_bcLocal = new BridgeConnection(idConnection, true, local, remote, bl, connConverter);
			_bcRemote = new BridgeConnection(idConnection,false, remote, local, bl, connConverter);
        }
        #endregion

		#region IBridgeConnection
		public void Start()
        {
            if (_bcLocal == null || _bcRemote == null) return;
            _bcLocal.Start();
            _bcRemote.Start();
        }

        public void Stop()
        {
            if (_bcLocal != null) _bcLocal.Stop();
            if (_bcRemote != null) _bcRemote.Stop();
        }

        public bool Running
        {
            get
            {
                return (_bcLocal != null && _bcLocal.Running && _bcRemote != null && _bcRemote.Running);
            }
			private set
			{
				//nulla da fare
			}
        }
        #endregion
    }
}
