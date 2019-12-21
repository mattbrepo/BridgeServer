using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BridgeServer
{
	interface IBridgeConnection
	{
		bool Running { get; }
		void Start();
		void Stop();
	}
}
