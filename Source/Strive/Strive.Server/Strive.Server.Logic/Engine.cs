using System;
using System.Linq;
using System.Reflection;
using Common.Logging;
using Strive.Common;
using Strive.Network.Messaging;


namespace Strive.Server.Logic
{
    public class Engine
    {
        readonly MessageProcessor _messageProcessor;

        readonly StoppableThread _engineThread;
        readonly ILog _log = LogManager.GetCurrentClassLogger();

        public ServerStatusModel ServerStatusModel { get; private set; }

        public Engine(MessageProcessor messageProcessor)
        {
            _log.Info("Creating " + Assembly.GetExecutingAssembly().FullName);
            _engineThread = new StoppableThread(UpdateLoop);
            _messageProcessor = messageProcessor;
            ServerStatusModel = new ServerStatusModel { Status = "Created" };
        }

        public void Start()
        {
            ServerStatusModel.Status = "Starting";
            ServerStatusModel.Started = Global.Now;
            _messageProcessor.Listener.Start();
            _engineThread.Start();
            ServerStatusModel.Status = "Running";
        }

        public void Stop()
        {
            ServerStatusModel.Status = "Stopping";
            _engineThread.Stop();
            _messageProcessor.Listener.Stop();
            _log.Info("Server stopped.");
            ServerStatusModel.Status = "Stopped";
        }

        public void UpdateLoop()
        {
            try
            {
                // handle world changes
                Global.Now = DateTime.Now;
                _messageProcessor.World.Update(Global.Now);

                // handle incoming messages
                _messageProcessor.ProcessMessages();

                CleanupLinkdead();

                if ((DateTime.Now - Global.Now).TotalSeconds > 1)
                    _log.Warn("An update cycle took longer than one second.");
                else
                    System.Threading.Thread.Sleep(100);
            }
            catch (Exception e)
            {
                ServerStatusModel.Status = "Crashing";
                _log.Error("Update loop exception caught", e);
                Stop();
                ServerStatusModel.Status = "Crashed";
            }
        }

        void CleanupLinkdead()
        {
            var remove = _messageProcessor.Listener.Clients
                .Where(client => client.Status != ConnectionStatus.Connected
                    && (Global.Now - client.LastMessageTimestamp).TotalMinutes > 1)
                .ToList();
            foreach (ClientConnection client in remove)
            {
                if (client.Avatar != null)
                {
                    var a = client.Avatar;
                    _messageProcessor.World.RemoveClient(client);
                    // TODO: do I want their avatar removed or not?
                    //_messageProcessor.World.Remove(a);
                }
                else
                    client.Avatar = null;
                _messageProcessor.Listener.Clients.Remove(client);
            }
        }
    }
}
