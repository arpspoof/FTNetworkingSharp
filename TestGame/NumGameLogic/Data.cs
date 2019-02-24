using System;
using Infra.DataService.Protocol;

namespace NumGame.Logic
{
    public class PlayerInfo
    {
        public bool active;
    }
    
    public class Data : LeafDataProtocol
    {
        public PlayerInfo[] players;
        public int turn;
    }

    [Serializable]
    public class Count : LeafDataProtocol
    {
        public int cnt;
    }

    [Serializable]
    public class Err : LeafDataProtocol
    {
        public string msg;
    }

    [Serializable]
    public class Num : LeafDataProtocol
    {
        public int x;
    }

    [Serializable]
    public class Turn : LeafDataProtocol
    {
        public int i;
        public int num;
    }

    [Serializable]
    public class End : LeafDataProtocol { }
}
