using System;
using System.Threading;
using System.Net;
using System.Collections;
using Strive.Network.Server;

namespace Strive.Server {
	/// <summary>
	/// </summary>
	class TehServ0r {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main( string[] args ) {
			if ( System.Configuration.ConfigurationSettings.AppSettings["port"] == null ) {
				throw new System.Configuration.ConfigurationException( "port" );
			}
			int port = int.Parse(System.Configuration.ConfigurationSettings.AppSettings["port"]);

			Queue packetQueue = new Queue();
			Listener listener = new Listener(
				new IPEndPoint( IPAddress.Any, port )
			);
			listener.Start();
			
			World world = new World();
			MessageProcessor mp = new MessageProcessor( world, listener	);

			while ( true ) {
				// handle world changes
				Global.now = DateTime.Now;
				world.Update();

				// handle incomming messages
				while ( listener.MessageCount > 0 ) {
					mp.ProcessNextMessage();
				}
				mp.CleanupDeadConnections();

				if ( (DateTime.Now - Global.now) > TimeSpan.FromSeconds(1) ) {
					System.Console.WriteLine( "WARNING: an update cycle took longer than one second" );
				} else {
					Thread.Sleep( 100 );
				}
			}
			Console.ReadLine();
		}
	}
}
