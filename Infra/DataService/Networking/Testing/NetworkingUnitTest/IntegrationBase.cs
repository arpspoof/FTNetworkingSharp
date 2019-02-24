using System.Runtime.Serialization.Formatters.Binary;
using Infra.DataService.Protocol;
using Infra.DataService.Protocol.UnitTest;

namespace Infra.DataService.Networking.UnitTest
{
    public class StateProvider : IStateDataProvider
    {
        AbstractProtocol IStateDataProvider.GetStateRecoveryData() => new MockData2();
    }

    public class IntegrationBase
    {
        public MockWire wire = new MockWire();
        public MockTCP server, client;
        public StateProvider serverStateProvider = new StateProvider();
        public ProtocolTree serverTree = new ProtocolTree(), clientTree = new ProtocolTree();
        public FTConnectionController serverStatefulSession, clientStatelessSession;
        public DataSerializer serverSerializer, clientSerializer;
        public LeafProtocolHandler<MockData1> serverData, clientData;
        public LeafProtocolHandler<MockData2> clientRecovery;
        public int serverDataCount = 0, clientDataCount = 0, recoveryCount = 0;
        public IntegrationBase()
        {
            server = new MockTCP(wire);
            client = new MockTCP(wire);

            server.other = client;
            client.other = server;

            serverStatefulSession = FTConnectionController.CreateStatefulSession(server, new StateProvider(), 5000, 8000);
            clientStatelessSession = FTConnectionController.CreateStatelessSession(client, 5000, 8000);
            
            serverTree.AttachErrorHandler();
            clientTree.AttachErrorHandler();

            serverSerializer = new DataSerializer();
            clientSerializer = new DataSerializer();
            serverSerializer.AttachErrorHandler();
            clientSerializer.AttachErrorHandler();

            serverTree.ConnectToDownstream(serverSerializer);
            serverSerializer.ConnectToDownstream(server);
            server.ConnectToUpstream(serverSerializer);
            serverSerializer.ConnectToUpstream(serverTree);

            clientTree.ConnectToDownstream(clientSerializer);
            clientSerializer.ConnectToDownstream(client);
            client.ConnectToUpstream(clientSerializer);
            clientSerializer.ConnectToUpstream(clientTree);

            serverData = new LeafProtocolHandler<MockData1>();
            clientData = new LeafProtocolHandler<MockData1>();
            clientRecovery = new LeafProtocolHandler<MockData2>();

            serverTree.Register(serverStatefulSession);
            serverTree.Register(serverData);
            serverTree.Entry(serverStatefulSession);
            serverTree.ConnectToLeaf(serverStatefulSession, serverData);

            clientTree.Register(clientStatelessSession);
            clientTree.Register(clientData);
            clientTree.Register(clientRecovery);
            clientTree.Entry(clientStatelessSession);
            clientTree.ConnectToLeaf(clientStatelessSession, clientData);
            clientTree.ConnectToLeaf(clientStatelessSession, clientRecovery);

            serverData.NewData += protocol => { protocol.Verify(); serverDataCount++; };
            clientData.NewData += protocol => { protocol.Verify(); clientDataCount++; };
            clientRecovery.NewData += protocol => { protocol.Verify(); recoveryCount++; };
        }
        public void ServerSend() => serverData.Send(new MockData1());
        public void ClientSend() => clientData.Send(new MockData1());
        public void MutualSend() { ServerSend(); ClientSend(); }
        public void Done() { serverStatefulSession.Dispose(); clientStatelessSession.Dispose(); }
    }
}
