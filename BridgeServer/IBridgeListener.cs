using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BridgeServer
{
	interface IBridgeListener
	{
		SetMessageDelegate OnMessage { get; set; }
		SetMessageDelegate OnConnectionNumberChange { get; set; }
		SetConnectionMessageDelegate OnConnectionMessage { get; set; }
		SetConnectionMessageByteArrayDelegate OnConnectionMessageByteArray { get; set; }

		void UpdateConnectionStatus();
	}
}
