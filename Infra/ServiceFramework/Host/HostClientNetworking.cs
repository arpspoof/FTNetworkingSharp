using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Infra.DataService.Networking;
using Infra.DataService.Protocol;
using static Infra.Logger;

namespace Infra.ServiceFramework.Host
{
    public partial class ServiceHost
    {
        private class DummyProvider : IStateDataProvider
        {
            public AbstractProtocol GetStateRecoveryData() => null;
        }

        private DynamicListener listener;
        private readonly Dictionary<ApplicationConnectionManager, int> applicationConnections
            = new Dictionary<ApplicationConnectionManager, int>();
        private readonly Dictionary<int, System.Timers.Timer> connectionInterruptTimers
            = new Dictionary<int, System.Timers.Timer>();
        private readonly Dictionary<int, long> resumeTokens = new Dictionary<int, long>();

        private void ListenerTask()
        {
            while (true)
            {
                Log("trying to accept a new client ...");
   
                DynamicRemoteClient remoteClient = null;
                try
                {
                    remoteClient = listener.Accept();
                }
                catch
                {
                    Log("unable to accept client", LogType.ERROR);
                    Thread.Sleep(2000);
                    continue;
                }

                ProtocolTree tree = new ProtocolTree();
                DummyHandler<DummyProtocol> treeRoot = new DummyHandler<DummyProtocol>();
                LeafProtocolHandler<AuthenticationProtocol> authHandler
                    = new LeafProtocolHandler<AuthenticationProtocol>();
                tree.Register(treeRoot);
                tree.Register(authHandler);
                tree.Entry(treeRoot);
                tree.ConnectToLeaf(treeRoot, authHandler);

                ApplicationConnectionManager app =
                    new ApplicationConnectionManager(remoteClient, tree, new DummyProvider(), 3000, 6000);

                applicationConnections.Add(app, -1);

                authHandler.NewData += data =>
                {
                    if (data.statusCode != AuthenticationProtocol.StatusCode.Ack)
                    {
                        Log("invalid auth status code: ACK", LogType.ERROR);
                        return;
                    }
                    void accept()
                    {
                        long resumeToken = 0;
                        if (!resumeTokens.ContainsKey(data.interfaceId))
                        {
                            resumeToken = Rnd64();
                            resumeTokens.Add(data.interfaceId, resumeToken);
                        }
                        else resumeTokens[data.interfaceId] = resumeTokens[data.interfaceId];
                        Log($"accept the client {data.interfaceId}, token = {resumeToken}");

                        authHandler.Send(new AuthenticationProtocol
                        {
                            statusCode = AuthenticationProtocol.StatusCode.Accept,
                            resumeToken = resumeToken
                        });
                    }
                    void reject(string reason)
                    {
                        authHandler.Send(new AuthenticationProtocol
                        {
                            statusCode = AuthenticationProtocol.StatusCode.Reject,
                            reason = reason
                        });
                    }
                    if (data.interfaceId < 0 || data.interfaceId >= logic.MaxConnection)
                    {
                        Log($"reject the client since the interface {data.interfaceId} is not valie", LogType.WARNING);
                        reject("invalid interface id");
                        return;
                    }

                    lock (interfaceLock)
                    {
                        bool IsExistingUser = connectionInterruptTimers.ContainsKey(data.interfaceId);
                        if (IsExistingUser && data.resumeToken != resumeTokens[data.interfaceId])
                        {
                            Log($"reject the client since the resume token {data.resumeToken} is not correct", LogType.WARNING);
                            reject("wrong resume token");
                            return;
                        }
                        if (!serviceBackupData.connectionInterfaces[data.interfaceId].Enabled && !IsExistingUser)
                        {
                            Log($"reject the client since the interface {data.interfaceId} is not available", LogType.WARNING);
                            reject("interface is not available");
                            return;
                        }
                        if (serviceBackupData.connectionInterfaces[data.interfaceId].Ticket != data.ticket)
                        {
                            Log($"reject the client since the ticket {data.ticket} is not correct", LogType.WARNING);
                            reject("invalid ticket");
                            return;
                        }

                        Log("auth is valid, accept the client");
                        accept();

                        tree.Connect(treeRoot, logic.GetProtocolTree(data.interfaceId));
                        applicationConnections[app] = data.interfaceId;

                        if (IsExistingUser)
                        {
                            Log($"client with id {data.interfaceId} comes back", LogType.WARNING);
                            RemoveTimer(data.interfaceId);
                            logic.OnClientResume(data.interfaceId);
                        }
                        else
                        {
                            Log($"calling logic OnClientEnter and close interface {data.interfaceId}");
                            CloseInterface(data.interfaceId);
                            logic.OnClientEnter(data.interfaceId);
                        }
                    }
                };

                app.ConnectionLost += () =>
                {
                    app.Dispose();
                    int id = applicationConnections[app];
                    applicationConnections.Remove(app);

                    Log($"client {id} is leaving", LogType.WARNING);

                    if (id >= 0)
                    {
                        logic.GetProtocolTree(id).Detach();

                        if (logic.CanResume(id))
                        {
                            System.Timers.Timer timer = new System.Timers.Timer(20000) { AutoReset = false };
                            timer.Elapsed += (s, e) =>
                            {
                                lock (interfaceLock)
                                {
                                    Log($"client {id} leave permanently", LogType.WARNING);
                                    timer.Dispose();
                                    connectionInterruptTimers.Remove(id);
                                    resumeTokens.Remove(id);
                                    logic.OnClientLeave(id);
                                }
                            };
                            timer.Start();
                            connectionInterruptTimers.Add(id, timer);
                            logic.OnClientDisconnect(id);
                            Log($"start timer to wait for client {id} to come back", LogType.WARNING);
                        }
                        else
                        {
                            logic.OnClientLeave(id);
                            Log($"client {id} leave permanently since logic cannot resume now", LogType.WARNING);
                        }
                    }
                };

                remoteClient.Activate();

                Logger.Log("sending auth data ...", "ServiceHost");
                authHandler.Send(new AuthenticationProtocol { statusCode = AuthenticationProtocol.StatusCode.Request });
            }
        }

        public void InitClientListenerByTCP(IPAddress localAddress, int port)
        {
            listener = new DynamicListener(localAddress, port);
            listener.Start();
            new Task(ListenerTask).Start();
        }

        private void RemoveTimer(int id)
        {
            connectionInterruptTimers[id].Stop();
            connectionInterruptTimers[id].Dispose();
            connectionInterruptTimers.Remove(id);
        }
    }
}
