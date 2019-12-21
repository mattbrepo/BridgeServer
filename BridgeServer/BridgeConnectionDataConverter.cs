using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BridgeServer
{
    /// <summary>
    /// Convertitore dell'indirizzo all'interno del pacchetto dati
    /// </summary>
    class BridgeConnectionDataConverter
    {
		#region field
		private byte[] _localPattern1;
		private byte[] _localPattern2;
		private byte[] _remotePattern1;
		private byte[] _remotePattern2; 
		#endregion

		#region ctor
		public BridgeConnectionDataConverter(string localServer, int localPort, string remoteServer, int remotePort)
		{
			//pattern1: solo indirizzo
			_localPattern1 = System.Text.Encoding.ASCII.GetBytes(localServer);
			_remotePattern1 = System.Text.Encoding.ASCII.GetBytes(remoteServer);

			//pattern2: indirizzo:port
			_localPattern2 = System.Text.Encoding.ASCII.GetBytes(localServer + ":" + localPort);
			_remotePattern2 = System.Text.Encoding.ASCII.GetBytes(remoteServer + ":" + remotePort);
		} 
		#endregion

		#region func

		public void Convert(ref byte[] bytes, ref int length, bool local)
		{
			if (local)
			{
				Convert(ref bytes, ref length, _localPattern2, _remotePattern2);
				Convert(ref bytes, ref length, _localPattern1, _remotePattern1);
			}
			else
			{
				Convert(ref bytes, ref length, _remotePattern2, _localPattern2);
				Convert(ref bytes, ref length, _remotePattern1, _localPattern1);
			}
		}

		private bool Convert(ref byte[] bytes, ref int length, byte[] patternSearch, byte[] patternSubstitute)
		{
			while (true)
			{
				int pos = SimpleBoyerMooreSearch(bytes, length, patternSearch);
				if (pos < 0) break;
				ArraySubstitute(ref bytes, ref length, pos, patternSearch.Length, patternSubstitute);
			}

			return false;
		}

		private void ArraySubstitute(ref byte[] bytes, ref int length, int pos, int patternSearchLength, byte[] patternSubstitute)
		{
			length += patternSubstitute.Length - patternSearchLength;
			byte[] bytesSub = new byte[length];

			Array.Copy(bytes, bytesSub, pos);
			Array.Copy(patternSubstitute, 0, bytesSub, pos, patternSubstitute.Length);

			try
			{
				Array.Copy(bytes, pos + patternSearchLength, bytesSub, pos + patternSubstitute.Length, length - pos - patternSubstitute.Length);
			}
			catch (Exception)
			{
				//...non dovrebbe succededere
				System.Diagnostics.Debug.Assert(false);
			}

			bytes = bytesSub;
		}

		/// <summary>
		/// In computer science, the Boyer–Moore string search algorithm is an efficient string searching algorithm 
		/// that is the standard benchmark for practical string search literature.
		/// It was developed by Robert S. Boyer and J Strother Moore in 1977
		/// https://en.wikipedia.org/wiki/Boyer%E2%80%93Moore_string_search_algorithm
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="length"></param>
		/// <param name="pattern"></param>
		/// <returns></returns>
		private int SimpleBoyerMooreSearch(byte[] bytes, int length, byte[] pattern)
		{
			int[] lookup = new int[256];
			for (int i = 0; i < lookup.Length; i++) { lookup[i] = pattern.Length; }

			for (int i = 0; i < pattern.Length; i++)
			{
				lookup[pattern[i]] = pattern.Length - i - 1;
			}

			int index = pattern.Length - 1;
			var lastByte = pattern.Last();
			while (index < length)
			{
				var checkByte = bytes[index];
				if (bytes[index] == lastByte)
				{
					bool found = true;
					for (int j = pattern.Length - 2; j >= 0; j--)
					{
						if (bytes[index - pattern.Length + j + 1] != pattern[j])
						{
							found = false;
							break;
						}
					}

					if (found)
						return index - pattern.Length + 1;
					else
						index++;
				}
				else
				{
					index += lookup[checkByte];
				}
			}
			return -1;
		} 
		#endregion
    }
}
