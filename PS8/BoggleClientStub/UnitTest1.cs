﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BoggleClient;

namespace BoggleClientControllerTester
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IBoggleBoard stub = new BoggleClientStub();
            Controller controller = new Controller(stub, new StartForm());

        }
    }
}