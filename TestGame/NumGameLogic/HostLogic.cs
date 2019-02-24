using System;
using Infra.ServiceFramework.Host;

namespace NumGame.Logic
{
    public class HostLogic : AbstractServiceLogic
    {
        private readonly Data data = new Data();
        private int activePlayers = 0;
        private int nextNum = 0;
        private bool start = false;

        public HostLogic()
        {
            RegisterDataType<Num>((i, d) =>
            {
                if (!start) return;
                if (data.turn != i) Send(i, new Err { msg = "not your turn" });
                else if (nextNum != d.x) Send(i, new Err { msg = "wrong answer" });
                else
                {
                    data.turn = (data.turn + 1) % 2;
                    nextNum++;
                    Send(data.turn, new Turn { i = data.turn, num = nextNum });
                    Send(i, new Turn { i = data.turn });
                }
            });
            RegisterDataType<Count>(null);
            RegisterDataType<Err>(null);
            RegisterDataType<Turn>(null);
            RegisterDataType<End>(null);

            data.players = new PlayerInfo[2];
            data.players[0] = new PlayerInfo();
            data.players[1] = new PlayerInfo();
            data.turn = 0;
        }

        public override int MaxConnection => 2;

        public override void OnClientEnter(int index)
        {
            data.players[index].active = true;
            activePlayers++;
            Count c = new Count { cnt = activePlayers };
            Send(0, c);
            Send(1, c);
            if (activePlayers == 2)
            {
                start = true;
                Send(0, new Turn { i = data.turn, num = nextNum });
                Send(1, new Turn { i = data.turn });
            }
        }

        public override void OnClientLeave(int index)
        {
            start = false;
            data.turn = 0;
            nextNum = 0;
            data.players[index].active = false;
            activePlayers--;
            Send(0, new End());
            Send(1, new End()); 
            OpenInterface(0);
            OpenInterface(1);
        }

        public override void OnClientResume(int index)
        {
            Send(index, new Count { cnt = activePlayers });
            if (data.turn == index)
            {
                Send(index, new Turn { i = index, num = nextNum });
            }
        }

        public override void Init()
        {
        }

        public override bool CanResume(int index) => start;

        public override void OnClientDisconnect(int index)
        {
            Console.WriteLine($"disconnect {index}, waiting ...");
        }
    }
}
