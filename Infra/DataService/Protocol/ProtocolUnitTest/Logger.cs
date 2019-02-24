using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Infra.DataService.Protocol.UnitTest
{
    public class Logger
    {
        private List<string> msgs = new List<string>();

        public void Log(string msg) => msgs.Add(msg);

        public void Verify(List<string> exp)
        {
            if (msgs.Count != exp.Count)
                Assert.Fail($"expect {exp.Count} msgs, but have {msgs.Count}");
            for (int i = 0; i < msgs.Count; i++)
            {
                if (msgs[i] != exp[i])
                    Assert.Fail($"comparing {i}: expect {exp[i]} but have {msgs[i]}");
            }
        }

        public void Clear() => msgs.Clear();
    }
}
