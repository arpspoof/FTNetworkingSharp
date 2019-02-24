using System;
using System.Net;
using Infra.DataService.Networking;
using Infra.DataService.Protocol;

namespace Infra.ServiceFramework.Client
{
    public class ServiceClient
    {
        public event Action ConnectionAccepted;
        public event Action ConnectionRejected;
        public event Action ConnectionLost;

        public long ResumeToken { get; set; }

        private bool rejected = false;

        private readonly IClientLogic logic;
        private readonly DynamicClient client;
        private readonly ProtocolTree tree;
        private readonly DummyHandler<DummyProtocol> root;
        private readonly LeafProtocolHandler<AuthenticationProtocol> auth;
        private readonly ApplicationConnectionManager app;

        public ServiceClient(IClientLogic logic, IPAddress hostIP, int port, int interfaceId, long ticket)
        {
            this.logic = logic;

            client = new DynamicClient(hostIP, port);
            tree = new ProtocolTree();
            root = new DummyHandler<DummyProtocol>();
            auth = new LeafProtocolHandler<AuthenticationProtocol>();

            tree.Register(root);
            tree.Register(auth);
            tree.Entry(root);
            tree.ConnectToLeaf(root, auth);
            tree.Connect(root, logic.ProtocolTree);

            app = new ApplicationConnectionManager(client, tree, 3000, 6000);

            auth.NewData += data =>
            {
                switch (data.statusCode)
                {
                    case AuthenticationProtocol.StatusCode.Request:
                        Logger.Log("receiving auth request", "ServiceClient");
                        auth.Send(new AuthenticationProtocol
                        {
                            interfaceId = interfaceId,
                            ticket = ticket,
                            resumeToken = ResumeToken,
                            statusCode = AuthenticationProtocol.StatusCode.Ack
                        });
                        break;
                    case AuthenticationProtocol.StatusCode.Accept:
                        Logger.Log("auth accepted by the host", "ServiceClient");
                        ResumeToken = data.resumeToken;
                        ConnectionAccepted?.Invoke();
                        break;
                    case AuthenticationProtocol.StatusCode.Reject:
                        Logger.Log($"auth rejected by the host, {data.reason}", "ServiceClient", Logger.LogType.WARNING);
                        rejected = true;
                        client.CloseConnection();
                        app.Dispose();
                        ConnectionRejected?.Invoke();
                        break;
                    default:
                        Logger.Log("invalid auth msg from host", "ServiceClient", Logger.LogType.WARNING);
                        break;
                }
            };

            app.ConnectionLost += () =>
            {
                if (!rejected) ConnectionLost?.Invoke();
            };
        }

        public bool Connect() => client.Connect();
    }
}
