using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Strive.Logging {
	/// <summary>
	/// Summary description for Log.
	/// </summary>
	public class Log {
		private TextBoxBase output = null;

		public Log() {
		}

		public Log( TextBoxBase output ) {
			this.output = output;
		}

		public void SetLogOutput( TextBoxBase output ) {
			this.output = output;
		}

		public void LogMessage( string message ) {
			message = "[" + DateTime.Now.Hour.ToString().PadLeft(2, '0') + ":" 
				+ DateTime.Now.Minute.ToString().PadLeft(2, '0') + ":" 
				+ DateTime.Now.Second.ToString().PadLeft(2, '0') + "]" + message;
			Trace.WriteLine( message );
			StringAppendFinite( message );
		}

		public void ErrorMessage( string message ) {
			LogMessage( "ERROR: "+message );
		}

		public void WarningMessage( string message ) {
			LogMessage( "WARNING: "+message );
		}

		public void DebugMessage( string message ) {
			Debug.WriteLine( message );
			StringAppendFinite( message );
		}

		private void StringAppendFinite( string message ) {
			if ( output != null ) {
				output.Text = message + Environment.NewLine + output.Text;
				if ( output.Text.Length > 1000 ) {
					output.Text = output.Text.Remove(1000, output.Text.Length - 1000);
				}
			}
		}

	}
}