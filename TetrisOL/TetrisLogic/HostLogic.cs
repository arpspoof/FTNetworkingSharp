using System;
using System.Collections.Generic;
using Infra.ServiceFramework.Host;

namespace Tetris.Logic
{
    public class TetrisHostLogic : AbstractServiceLogic
    {
        public override int MaxConnection => throw new NotImplementedException();

        public override bool CanResume(int index)
        {
            throw new NotImplementedException();
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public override void OnClientDisconnect(int index)
        {
            throw new NotImplementedException();
        }

        public override void OnClientEnter(int index)
        {
            throw new NotImplementedException();
        }

        public override void OnClientLeave(int index)
        {
            throw new NotImplementedException();
        }

        public override void OnClientResume(int index)
        {
            throw new NotImplementedException();
        }
    }
}
