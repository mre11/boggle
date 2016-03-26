using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Dynamic;
using static System.Net.HttpStatusCode;
using System.Diagnostics;

namespace Boggle
{
    /// <summary>
    /// Provides a way to start and stop the IIS web server from within the test
    /// cases.  If something prevents the test cases from stopping the web server,
    /// subsequent tests may not work properly until the stray process is killed
    /// manually.
    /// </summary>
    public static class IISAgent
    {
        // Reference to the running process
        private static Process process = null;

        /// <summary>
        /// Starts IIS
        /// </summary>
        public static void Start(string arguments)
        {
            if (process == null)
            {
                ProcessStartInfo info = new ProcessStartInfo(Properties.Resources.IIS_EXECUTABLE, arguments);
                info.WindowStyle = ProcessWindowStyle.Minimized;
                info.UseShellExecute = false;
                process = Process.Start(info);
            }
        }

        /// <summary>
        ///  Stops IIS
        /// </summary>
        public static void Stop()
        {
            if (process != null)
            {
                process.Kill();
            }
        }
    }
    [TestClass]
    public class BoggleTests
    {
        /// <summary>
        /// This is automatically run prior to all the tests to start the server
        /// </summary>
        [ClassInitialize()]
        public static void StartIIS(TestContext testContext)
        {
            IISAgent.Start(@"/site:""BoggleService"" /apppool:""Clr4IntegratedAppPool"" /config:""..\..\..\.vs\config\applicationhost.config""");
        }

        /// <summary>
        /// This is automatically run when all tests have completed to stop the server
        /// </summary>
        [ClassCleanup()]
        public static void StopIIS()
        {
            IISAgent.Stop();
        }

        private RestTestClient client = new RestTestClient("http://localhost:60000/");

        [TestMethod]
        public void TestMethod1()
        {
            Response r = client.DoGetAsync("/numbers?length={0}", "5").Result;
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual(5, r.Data.Count);
            r = client.DoGetAsync("/numbers?length={0}", "-5").Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        [TestMethod]
        public void TestMethod2()
        {
            List<int> list = new List<int>();
            list.Add(15);
            Response r = client.DoPostAsync("/first", list).Result;
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual(15, r.Data);
        }

        /// <summary>
        /// Test successful request
        /// </summary>
        [TestMethod]
        public void TestCreateUser1()
        {
            dynamic data = new ExpandoObject();
            data.Nickname = "Test";
            Response r = client.DoPostAsync("/users", data).Result;
            Assert.AreEqual(Created, r.Status);
            Assert.AreEqual(36, r.Data.UserToken.Length);
        }

        /// <summary>
        /// Test when Nickname is invalid
        /// </summary>
        [TestMethod]
        public void TestCreateUser2()
        {
            dynamic data = new ExpandoObject();
            data.Nickname = "";
            Response r = client.DoPostAsync("/users", data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when UserToken is invalid
        /// </summary>
        [TestMethod]
        public void TestJoinGame1()
        {
            dynamic data = new ExpandoObject();
            data.UserToken = "";
            data.TimeLimit = 10;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when TimeLimit is less than 5
        /// </summary>
        [TestMethod]
        public void TestJoinGame2()
        {
            dynamic data = new ExpandoObject();
            data.UserToken = Guid.NewGuid().ToString();
            data.TimeLimit = 4;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when TimeLimit is greater than 120
        /// </summary>
        [TestMethod]
        public void TestJoinGame3()
        {
            dynamic data = new ExpandoObject();
            data.UserToken = Guid.NewGuid().ToString();
            data.TimeLimit = 4;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test for a UserToken conflict
        /// </summary>
        [TestMethod]
        public void TestJoinGame4()
        {
            dynamic data = new ExpandoObject();
            var guid = Guid.NewGuid().ToString();
            data.UserToken = guid;
            data.TimeLimit = 10;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Conflict, r.Status);
        }

        /// <summary>
        /// Test a successful request
        /// </summary>
        [TestMethod]
        public void TestJoinGame5()
        {
            dynamic data = new ExpandoObject();
            data.UserToken = Guid.NewGuid().ToString();
            data.TimeLimit = 10;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            data.UserToken = Guid.NewGuid().ToString();
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Created, r.Status);
            Assert.AreEqual(gameID, (string)r.Data.GameID);
        }

        /// <summary>
        /// Test when UserToken is invalid
        /// </summary>
        [TestMethod]
        public void TestCancelJoin1()
        {
            dynamic data = new ExpandoObject();
            Response r = client.DoPutAsync("/games", data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when UserToken is not in pending game
        /// </summary>
        [TestMethod]
        public void TestCancelJoin2()
        {
            dynamic data = new ExpandoObject();
            data.UserToken = Guid.NewGuid().ToString();
            Response r = client.DoPutAsync("/games", data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test successful request
        /// </summary>
        [TestMethod]
        public void TestCancelJoin3()
        {
            var userToken = Guid.NewGuid().ToString();
            dynamic data = new ExpandoObject();
            data.UserToken = userToken;
            data.TimeLimit = 10;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            data = new ExpandoObject();
            data.UserToken = userToken;

            r = client.DoPutAsync("/games", data).Result;
            Assert.AreEqual(OK, r.Status);
        }

        /// <summary>
        /// Test when word is invalid
        /// </summary>
        [TestMethod]
        public void TestPlayWord1()
        {
            // First create a game
            dynamic data = new ExpandoObject();
            var userToken1 = Guid.NewGuid().ToString();
            data.UserToken = userToken1;
            data.TimeLimit = 10;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            var userToken2 = Guid.NewGuid().ToString();
            data.UserToken = userToken2;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Created, r.Status);
            Assert.AreEqual(gameID, (string)r.Data.GameID);

            // Do the put request
            data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "";

            r = client.DoPutAsync("/games/" + gameID, data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when GameID is invalid
        /// </summary>
        [TestMethod]
        public void TestPlayWord2()
        {
            // First create a game
            dynamic data = new ExpandoObject();
            var userToken1 = Guid.NewGuid().ToString();
            data.UserToken = userToken1;
            data.TimeLimit = 10;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            var userToken2 = Guid.NewGuid().ToString();
            data.UserToken = userToken2;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Created, r.Status);
            Assert.AreEqual(gameID, (string)r.Data.GameID);

            // Do the put request
            data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "hello";

            gameID = "test";

            r = client.DoPutAsync("/games/" + gameID, data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when UserToken is invalid
        /// </summary>
        [TestMethod]
        public void TestPlayWord3()
        {
            // First create a game
            dynamic data = new ExpandoObject();
            var userToken1 = Guid.NewGuid().ToString();
            data.UserToken = userToken1;
            data.TimeLimit = 10;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            var userToken2 = Guid.NewGuid().ToString();
            data.UserToken = userToken2;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Created, r.Status);
            Assert.AreEqual(gameID, r.Data.GameID.ToString());

            // Do the put request
            data = new ExpandoObject();
            data.UserToken = Guid.NewGuid().ToString();
            data.Word = "hello";

            r = client.DoPutAsync("/games/" + gameID, data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when game state is not active
        /// </summary>
        [TestMethod]
        public void TestPlayWord4()
        {
            // First create a game
            dynamic data = new ExpandoObject();
            var userToken1 = Guid.NewGuid().ToString();
            data.UserToken = userToken1;
            data.TimeLimit = 5;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            //var userToken2 = Guid.NewGuid().ToString();
            //data.UserToken = userToken2;
            //r = client.DoPostAsync("/games", data).Result;
            //Assert.AreEqual(Created, r.Status);
            //Assert.AreEqual(gameID, (string)r.Data.GameID);

            // Do the put request
            data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "hello";

            r = client.DoPutAsync("/games/" + gameID, data).Result;
            Assert.AreEqual(Conflict, r.Status);
        }

        /// <summary>
        /// Test a successful request
        /// </summary>
        [TestMethod]
        public void TestPlayWord5()
        {
            // First create a game
            dynamic data = new ExpandoObject();
            var userToken1 = Guid.NewGuid().ToString();
            data.UserToken = userToken1;
            data.TimeLimit = 5;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            var userToken2 = Guid.NewGuid().ToString();
            data.UserToken = userToken2;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Created, r.Status);
            Assert.AreEqual(gameID, (string)r.Data.GameID);

            // Do the put request
            data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "asdf";

            r = client.DoPutAsync("/games/" + gameID, data).Result;
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual("-1", r.Data.Score.ToString());
        }
    }
}
