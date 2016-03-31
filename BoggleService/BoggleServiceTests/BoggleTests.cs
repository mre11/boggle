using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Dynamic;
using static System.Net.HttpStatusCode;
using System.Diagnostics;
using System.Threading;

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
        //private RestTestClient client = new RestTestClient("http://bogglecs3500s16.azurewebsites.net/");

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

            string token = r.Data.UserToken;
            Assert.AreEqual(36, token.Length);
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

            data.Nickname = null;
            r = client.DoPostAsync("/users", data).Result;
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
            // First create a user
            dynamic data = new ExpandoObject();
            data.Nickname = "Test";
            Response r = client.DoPostAsync("/users", data).Result;
            string userToken = r.Data.UserToken;

            // Join game with the user
            data = new ExpandoObject();
            data.UserToken = userToken;
            data.TimeLimit = 4;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when TimeLimit is greater than 120
        /// </summary>
        [TestMethod]
        public void TestJoinGame3()
        {
            // First create a user
            dynamic data = new ExpandoObject();
            data.Nickname = "Test";
            Response r = client.DoPostAsync("/users", data).Result;
            string userToken = r.Data.UserToken;

            // Join game with the user
            data = new ExpandoObject();
            data.UserToken = userToken;
            data.TimeLimit = 4;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test for a UserToken conflict
        /// </summary>
        [TestMethod]
        public void TestJoinGame4()
        {
            // First create a user
            dynamic data = new ExpandoObject();
            data.Nickname = "Test";
            Response r = client.DoPostAsync("/users", data).Result;
            string userToken = r.Data.UserToken;

            // Join game with the user
            data = new ExpandoObject();
            data.UserToken = userToken;
            data.TimeLimit = 10;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            // Try to join the game with the same user
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Conflict, r.Status);
        }

        /// <summary>
        /// Test a successful request
        /// </summary>
        [TestMethod]
        public void TestJoinGame5()
        {
            StartBoggleGame(10);
        }

        /// <summary>
        /// Test when UserToken is invalid
        /// </summary>
        [TestMethod]
        public void TestCancelJoin1()
        {
            dynamic data = new ExpandoObject();
            Response r = client.DoPutAsync(data, "/games").Result;
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
            Response r = client.DoPutAsync(data, "/games").Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test successful request
        /// </summary>
        [TestMethod]
        public void TestCancelJoin3()
        {
            // First create a user
            dynamic data = new ExpandoObject();
            data.Nickname = "Test";
            Response r = client.DoPostAsync("/users", data).Result;
            string userToken = r.Data.UserToken;

            // Join game with the first user
            data = new ExpandoObject();
            data.UserToken = userToken;
            data.TimeLimit = 10;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            // Cancel the join request
            data = new ExpandoObject();
            data.UserToken = userToken;

            r = client.DoPutAsync(data, "/games").Result;
            Assert.AreEqual(OK, r.Status);
        }

        /// <summary>
        /// Test when word is invalid
        /// </summary>
        [TestMethod]
        public void TestPlayWord1()
        {
            // Start a game
            string[] result = StartBoggleGame(5);
            string gameID = result[0];
            string userToken1 = result[1];

            // Do the put request
            dynamic data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "";

            Response r = client.DoPutAsync(data, "/games/" + gameID).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when GameID is invalid
        /// </summary>
        [TestMethod]
        public void TestPlayWord2()
        {
            // Start a game
            string[] result = StartBoggleGame(5);
            string gameID = result[0];
            string userToken1 = result[1];

            // Do the put request
            dynamic data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "hello";

            gameID = "test";

            Response r = client.DoPutAsync(data, "/games/" + gameID).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when UserToken is invalid
        /// </summary>
        [TestMethod]
        public void TestPlayWord3()
        {
            // Start a game
            string[] result = StartBoggleGame(5);
            string gameID = result[0];
            string userToken1 = result[1];

            // Do the put request
            dynamic data = new ExpandoObject();
            data.UserToken = Guid.NewGuid().ToString();
            data.Word = "hello";

            Response r = client.DoPutAsync(data, "/games/" + gameID).Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when game state is not active
        /// </summary>
        [TestMethod]
        public void TestPlayWord4()
        {
            // Start a game
            string[] result = StartBoggleGame(5);
            string gameID = result[0];
            string userToken1 = result[1];
            Thread.Sleep(6000); // make sure the game is completed

            // Do the put request
            dynamic data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "hello";

            Response r = client.DoPutAsync(data, "/games/" + gameID).Result;
            Assert.AreEqual(Conflict, r.Status);
        }

        /// <summary>
        /// Test a successful request
        /// </summary>
        [TestMethod]
        public void TestPlayWord5()
        {
            // Start a game
            string[] result = StartBoggleGame(60);
            string gameID = result[0];
            string userToken1 = result[1];

            // Do the put request
            dynamic data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "asdf";

            Response r = client.DoPutAsync(data, "/games/" + gameID).Result;
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual(-1, (int)r.Data.Score);
        }

        /// <summary>
        /// Test a successful request
        /// </summary>
        [TestMethod]
        public void TestPlayWord6()
        {
            // Start a game
            string[] result = StartBoggleGame(60);
            string gameID = result[0];
            string userToken1 = result[1];

            // Do the put request
            dynamic data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "asdf";

            Response r = client.DoPutAsync(data, "/games/" + gameID).Result;
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual(-1, (int)r.Data.Score);
        }

        // TODO need more PlayWord tests to test scoring

        /// <summary>
        /// Test when GameID is invalid
        /// </summary>
        [TestMethod]
        public void TestGameStatus1()
        {
            Response r = client.DoGetAsync("/games/badID").Result;
            Assert.AreEqual(Forbidden, r.Status);
        }

        /// <summary>
        /// Test when GameState is pending
        /// </summary>
        [TestMethod]
        public void TestGameStatus2()
        {
            // First create a pending game
            dynamic data = new ExpandoObject();
            data.Nickname = "Test";
            Response r = client.DoPostAsync("/users", data).Result;
            string userToken1 = r.Data.UserToken;

            // Join game with the first user
            data = new ExpandoObject();
            data.UserToken = userToken1;
            data.TimeLimit = 5;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            // Do the get request
            r = client.DoGetAsync("/games/" + gameID).Result;
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual("pending", r.Data.GameState.ToString());
        }

        /// <summary>
        /// Test when Brief = "yes" and GameState is active
        /// </summary>
        [TestMethod]
        public void TestGameStatus3()
        {
            // Start a game
            string[] result = StartBoggleGame(60);
            string gameID = result[0];

            // Do the get request
            Response r = client.DoGetAsync("/games/" + gameID + "?Brief={0}", new string[] { "yes" }).Result;
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual("active", r.Data.gameState);
            Assert.AreNotEqual(null, r.Data.timeLeft);
            Assert.AreEqual(0, (int)r.Data.Player1.Score);
            Assert.AreEqual(0, (int)r.Data.Player2.Score);
        }

        /// <summary>
        /// Test when Brief = "yes" and GameState is completed
        /// </summary>
        [TestMethod]
        public void TestGameStatus4()
        {
            // Start a game
            string[] result = StartBoggleGame(5);
            string gameID = result[0];

            Thread.Sleep(6000); // make sure the game is completed

            // Do the get request
            Response r = client.DoGetAsync("/games/" + gameID + "?Brief={0}", new string[] { "yes" }).Result;
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual("completed", r.Data.gameState.ToString());
            Assert.AreEqual(0, (int)r.Data.timeLeft);
            Assert.AreEqual(0, (int)r.Data.Player1.Score);
            Assert.AreEqual(0, (int)r.Data.Player2.Score);
        }

        [TestMethod]
        public void TestGameStatus5()
        {
            string[] result = StartBoggleGame(50);
            string gameID = result[0];
            string userToken1 = result[1];

            // Do the put request
            dynamic data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "asdf";

            Response r = client.DoPutAsync(data, "/games/" + gameID).Result;

            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual(-1, (int)r.Data.Score);
        }

        /// <summary>
        /// Helper method that starts a Boggle game on the server.
        /// Returns an array containing GameID, Player1's UserToken, and Player2's UserToken.
        /// </summary>
        private string[] StartBoggleGame(int timeLimit)
        {
            // First create a user
            dynamic data = new ExpandoObject();
            data.Nickname = "Test";
            Response r = client.DoPostAsync("/users", data).Result;
            string userToken1 = r.Data.UserToken;

            // Join game with the first user
            data = new ExpandoObject();
            data.UserToken = userToken1;
            data.TimeLimit = timeLimit;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Accepted, r.Status);

            string gameID = r.Data.GameID;
            Assert.AreNotEqual(null, gameID);

            // Now create a second user
            data = new ExpandoObject();
            data.Nickname = "Test";
            r = client.DoPostAsync("/users", data).Result;
            string userToken2 = r.Data.UserToken;

            // Join game with the second user
            data = new ExpandoObject();
            data.UserToken = userToken2;
            data.TimeLimit = timeLimit;
            r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Created, r.Status);
            Assert.AreEqual(gameID, (string)r.Data.GameID);

            return new string[3] { gameID, userToken1, userToken2 };
        }

        // TODO still need more GameStatus tests (especially for non-Brief), plus any others for code coverage
    }
}
