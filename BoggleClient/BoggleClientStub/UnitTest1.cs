// Created for CS3500, Spring 2016
// Morgan Empey (U0634576), Braden Klunker (U0725294)

using System;
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
