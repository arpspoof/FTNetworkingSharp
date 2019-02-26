# FTNetworkingSharp
[C#] simple, easy to use, basic level of fault tolerance application layer connection middle-ware &amp; gaming framework 

[more document coming]

## Game Logic Dev Guide
#### Overview
***Sample code available in TestGame folder***
By using this networking framework, to develop a game we only need the logic module dll and GUI. Networking side is completely separated from the logic side so that the developer doesn't need to think about anything related to networking. In the game logic, basically you just need to decide when to send what data to which client. Also if needed, you need to add some specific logic to responde to events like *ClientDisconnect* or *ClientResume* which means that a user goes down and would like to resume the game.
### Server Side
#### Define your game data
[using Infra.DataService.Protocol]
1. Your data should be a C# class and must inherit from **LeafDataProtocol** in order to use the framework. 
2. Your data class must be marked as **[Serializable]**
3. Members in the data class must also be **[Serializable]**. You can embed another class inherited from **LeafDataProtocol** or **AbstractProtocol**. And you can use lots of built-in data structures: All **primitive types**, everything in **System.Collections.Generic**. Never include any non-serializable member variable inside this class. If you do want to do so, you should think about putting the non-serializable data into another class since it is not the data to be sent to clients.
4. You'll then be able to send and receive this type of data. See details below.
#### Define server side logic
[using Infra.ServiceFramework.Host]<br/>
Your logic should be a C# class and must inherit from **AbstractServiceLogic** or **IServiceLogic** so that it can be deployed to a generic game server. Things will be easier by using **AbstractServiceLogic** while it'll be more flexible if you use **IServiceLogic**.

##### Things to be overidden

**public abstract int MaxConnection { get; }**<br/>
You should return a constant indicating the number of clients.

**public abstract void Init();**<br/>
This function will be called when the logic module is successfully loaded. Do initializations here like registering data, add data listeners ... (details below). It's also possible to initialize in constructor if applicable then leave this function empty.

**public abstract void OnClientEnter(int index);**<br/>
Called when a new client is coming (first time, not for those who becomes disconnected and then comes back)

**public abstract void OnClientLeave(int index);**<br/>
Called when a client left permanently (force quiting or failed to come back after becoming disconnected)

**public abstract bool CanResume(int index);**<br/>
Called when a client is disconnected. Return true if you would like to wait for this client to come back. You can decide whether or not to pause the game or let other players continue on without being interrupted. If you return true, then a timer will be setup (60s by default) to wait for the disconnected client to come back. In this case, **OnClientDisconnect** will be called immediately and **OnClientLeave** or **OnClientResume** will be called later on depending on whether the client comes back or not. If the client comes back, you'd better send some recovery data in the **OnClientResume** function call. If you return false here, then **OnClientLeave** will then be called immediately.

**public abstract void OnClientDisconnect(int index);** [mentioned above]<br/>
**public abstract void OnClientResume(int index);** [mentioned above]<br/>

##### Helper functions to use to build up the logic module

**protected bool OpenInterface(int index)**<br/>
**protected bool CloseInterface(int index)**<br/>
These are connection interface control methods. The host can accept a client on an interface only if the interface is open. After initializing, all interfaces will be open by default. When a client is accepted on interface *i*, interface *i* will be closed immediately by default. A disconnected client can come back to the same interface if **CanResume** function allows to do so. There is no other default interface behaviors. If the game ends or interrupted unexpectedly or whatever, you'll have to call these functions to manage the interfaces yourself. 

**protected void RegisterDataType\<TData>(Action<int, TData> dataHandler) where TData : LeafDataProtocol**<br/>
For any game data you would like to use, you must register the type at init time. You'll be able to send or receive data of this type once it is registered. If you would like to receive this type of data, you must pass in a data handler as a receive callback. This callback takes two params, one integer means the interface id, one data object to be processed.

**protected void Send\<TData>(int i, TData data) where TData : LeafDataProtocol**<br/>
Send some data to client interface *i*. TData must be registered beforehand
