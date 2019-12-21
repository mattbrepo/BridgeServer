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
    /// Thread di connessione
    /// </summary>
    class BridgeConnection : IBridgeConnection
    {
        #region private
		private IBridgeListener _bl;
		private BridgeConnectionDataConverter _connConverter;
        
		private Thread _thread;
        private TcpClient _receiver;
        private TcpClient _sender;

		//private string _idStr;
        private bool _local; //local or remote
		private int _id; //connection id
        #endregion

        #region ctor
		public BridgeConnection(int id, bool local, TcpClient receiver, TcpClient sender, IBridgeListener bl, BridgeConnectionDataConverter connConverter)
        {
			if (bl == null) throw new Exception("BridgeListener is null!");

			_id = id;
			_local = local;

            _bl = bl;
			_connConverter = connConverter;

            _receiver = receiver;
			_sender = sender;
        }
        #endregion

		#region IBridgeConnection
		public void Start()
        {
            try
            {
                Running = true;

                _thread = new Thread(ComunicationLoop);
                _thread.IsBackground = true;
                _thread.Start();
            }
            catch (Exception)
            {
                Running = false;
            }
        }

        public void Stop()
        {
            Running = false;

            try
            {
                _receiver.Close();
            }
            catch (Exception) 
			{ 
			}

            try
            {
                _sender.Close();
            }
            catch (Exception) { }
            _bl.UpdateConnectionStatus();

			try
			{
				_thread.Abort();
			}
			catch (Exception)
			{
			}
        }

        public bool Running { get; private set; }
        #endregion

        private void ComunicationLoop()
        {
            Byte[] bytes = new Byte[1024];
            int count;
            NetworkStream readStream;
            NetworkStream writeStream;

			if (_bl.OnConnectionMessage != null) _bl.OnConnectionMessage(DateTime.Now, _local, _id, "connection started");

            try
            {
                readStream = _receiver.GetStream();
                writeStream = _sender.GetStream();

                while (Running && _receiver.Connected && _sender.Connected)
                {
                    //resta in attesa di dati dalla connessione
                    Array.Clear(bytes, 0, bytes.Length);
                    count = readStream.Read(bytes, 0, bytes.Length);
                    if (count == 0) break;
					DateTime dt = DateTime.Now;

					//converte gli indirizzi contenuti nel pacchetto dati se è presente un 'converter'
                    if (_connConverter != null) _connConverter.Convert(ref bytes, ref count, _local);                       

					if (_bl.OnConnectionMessageByteArray != null) _bl.OnConnectionMessageByteArray(dt, _local, _id, bytes, count);

                    writeStream.Write(bytes, 0, count);
                }

				if (Running && _bl.OnConnectionMessage != null) _bl.OnConnectionMessage(DateTime.Now, _local, _id, "connection terminated");
				Stop(); //giusto sempre o solo per HTTP?
            }
            catch (System.IO.IOException)
            {
                if (Running)
                {
					if (_bl.OnConnectionMessage != null) _bl.OnConnectionMessage(DateTime.Now, _local, _id, "connection terminated with exception");
                    Stop();
                }
            }
            catch (Exception)
            {
                if (Running)
                {
					if (_bl.OnConnectionMessage != null) _bl.OnConnectionMessage(DateTime.Now, _local, _id, "connection terminated, communication loop exception");
                    Stop();
                }
            }
        }
    }
}
