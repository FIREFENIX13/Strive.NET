
using System.Net;
using Strive.Common;
using Strive.Network.Messaging;
using Strive.Server.Logic;

namespace Strive.Server.WindowsService
{
    public class StriveService : System.ServiceProcess.ServiceBase
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        Engine striveEngine = new Engine(
            new MessageProcessor(
                new World(Global.WorldId),
                new Listener(new IPEndPoint(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0], Constants.DefaultPort))));

        public StriveService()
        {
            // This call is required by the Windows.Forms Component Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitComponent call
        }

        // The main entry point for the process
        static void Main()
        {
            System.ServiceProcess.ServiceBase[] ServicesToRun;

            // More than one user Service may run within the same process. To add
            // another service to this process, change the following line to
            // create a second service object. For example,
            //
            //   ServicesToRun = New System.ServiceProcess.ServiceBase[] {new Service1(), new MySecondUserService()};
            //
            ServicesToRun = new System.ServiceProcess.ServiceBase[] { new StriveService() };

            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ServiceName = "Strive.Server.WindowsService";
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Set things in motion so your service can do its work.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            striveEngine.Start();
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            striveEngine.Stop();
        }
    }
}
