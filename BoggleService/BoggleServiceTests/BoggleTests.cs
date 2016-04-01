// Braden Klunker, Morgan Empey, CS3500

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
        // private RestTestClient client = new RestTestClient("http://bogglecs3500s16.azurewebsites.net/");

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
        /// Test for a UserToken conflict
        /// </summary>
        [TestMethod]
        public void TestJoinGame5()
        {
            dynamic data = new ExpandoObject();
            data.UserToken = Guid.NewGuid();
            data.TimeLimit = 10;
            Response r = client.DoPostAsync("/games", data).Result;
            Assert.AreEqual(Conflict, r.Status);
        }

        /// <summary>
        /// Test a successful request
        /// </summary>
        [TestMethod]
        public void TestJoinGame6()
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
            string[] result = StartBoggleGame(120);
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
        /// Test for a valid user playing a word without first joining a game
        /// </summary>
        [TestMethod]
        public void TestPlayWord6()
        {
            string[] result = StartBoggleGame(15);
            string gameID = result[0];

            dynamic data = new ExpandoObject();
            data.Nickname = "Yuk";

            Response r = client.DoPostAsync("/users", data).Result;
            string userToken = r.Data.UserToken;

            dynamic playWord = new ExpandoObject();
            playWord.UserToken = userToken;
            playWord.Word = "work";
            Response playWordResponse = client.DoPutAsync(playWord, "/games/1").Result;

            Assert.AreEqual(Forbidden, playWordResponse.Status);
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
            // First create a user
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

            // Check that the right information was returned
            Assert.AreNotEqual(null, r.Data.GameState);
            Assert.AreEqual(null, r.Data.Board);
            Assert.AreEqual(null, r.Data.TimeLimit);
            Assert.AreEqual(null, r.Data.TimeLeft);
            Assert.AreEqual(null, r.Data.Player1);
            Assert.AreEqual(null, r.Data.Player2);

            // Check that the response is correct
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
            Thread.Sleep(1000); // make sure some time has passed
            Response r = client.DoGetAsync("/games/" + gameID + "?Brief={0}", new string[] { "yes" }).Result;

            // Check that the right information was returned
            Assert.AreNotEqual(null, r.Data.GameState);
            Assert.AreEqual(null, r.Data.Board);
            Assert.AreEqual(null, r.Data.TimeLimit);
            Assert.AreNotEqual(null, r.Data.TimeLeft);
            Assert.AreNotEqual(null, r.Data.Player1);
            Assert.AreNotEqual(null, r.Data.Player2);
            Assert.AreEqual(null, r.Data.Player1.Nickname);
            Assert.AreEqual(null, r.Data.Player2.Nickname);
            Assert.AreEqual(null, r.Data.Player1.UserToken);
            Assert.AreEqual(null, r.Data.Player2.UserToken);
            Assert.AreNotEqual(null, r.Data.Player1.Score);
            Assert.AreNotEqual(null, r.Data.Player2.Score);
            Assert.AreEqual(null, r.Data.Player1.WordsPlayed);
            Assert.AreEqual(null, r.Data.Player2.WordsPlayed);

            // Check that the response is correct
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual("active", r.Data.GameState.ToString());
            Assert.IsTrue((int)r.Data.TimeLeft > 0);
            Assert.IsTrue((int)r.Data.TimeLeft < 60);
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

            // Check that the right information was returned
            Assert.AreNotEqual(null, r.Data.GameState);
            Assert.AreEqual(null, r.Data.Board);
            Assert.AreEqual(null, r.Data.TimeLimit);
            Assert.AreNotEqual(null, r.Data.TimeLeft);
            Assert.AreNotEqual(null, r.Data.Player1);
            Assert.AreNotEqual(null, r.Data.Player2);
            Assert.AreEqual(null, r.Data.Player1.Nickname);
            Assert.AreEqual(null, r.Data.Player2.Nickname);
            Assert.AreEqual(null, r.Data.Player1.UserToken);
            Assert.AreEqual(null, r.Data.Player2.UserToken);
            Assert.AreNotEqual(null, r.Data.Player1.Score);
            Assert.AreNotEqual(null, r.Data.Player2.Score);
            Assert.AreEqual(null, r.Data.Player1.WordsPlayed);
            Assert.AreEqual(null, r.Data.Player2.WordsPlayed);

            // Check that the response is correct
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual("completed", r.Data.GameState.ToString());
            Assert.AreEqual(0, (int)r.Data.TimeLeft);
            Assert.AreEqual(0, (int)r.Data.Player1.Score);
            Assert.AreEqual(0, (int)r.Data.Player2.Score);
        }

        /// <summary>
        /// Test an active game when Brief is not yes
        /// </summary>
        [TestMethod]
        public void TestGameStatus5()
        {
            string[] result = StartBoggleGame(50);
            string gameID = result[0];
            string userToken1 = result[1];

            // Play a word
            dynamic data = new ExpandoObject();
            data.UserToken = userToken1;
            data.Word = "asdf";
            Response r = client.DoPutAsync(data, "/games/" + gameID).Result;

            // Get game status
            Thread.Sleep(1000); // make sure some time has passed
            r = client.DoGetAsync("/games/" + gameID, new string[1] { "" }).Result;

            // Check that the right information was returned
            Assert.AreNotEqual(null, r.Data.GameState);
            Assert.AreNotEqual(null, r.Data.Board);
            Assert.AreNotEqual(null, r.Data.TimeLimit);
            Assert.AreNotEqual(null, r.Data.TimeLeft);
            Assert.AreNotEqual(null, r.Data.Player1);
            Assert.AreNotEqual(null, r.Data.Player2);
            Assert.AreNotEqual(null, r.Data.Player1.Nickname);
            Assert.AreNotEqual(null, r.Data.Player2.Nickname);
            Assert.AreEqual(null, r.Data.Player1.UserToken);
            Assert.AreEqual(null, r.Data.Player2.UserToken);
            Assert.AreNotEqual(null, r.Data.Player1.Score);
            Assert.AreNotEqual(null, r.Data.Player2.Score);
            Assert.AreEqual(null, r.Data.Player1.WordsPlayed);
            Assert.AreEqual(null, r.Data.Player2.WordsPlayed);

            // Check that the response is correct
            Assert.AreEqual(OK, r.Status);
            Assert.AreEqual("active", r.Data.GameState.ToString());
            Assert.AreEqual(16, r.Data.Board.ToString().Length);
            Assert.AreEqual(50, (int)r.Data.TimeLimit);
            Assert.IsTrue((int)r.Data.TimeLeft > 0);
            Assert.IsTrue((int)r.Data.TimeLeft < 50);
            Assert.AreEqual("tester1", r.Data.Player1.Nickname.ToString());
            Assert.AreEqual("tester2", r.Data.Player2.Nickname.ToString());
            Assert.AreEqual(-1, (int)r.Data.Player1.Score);
            Assert.AreEqual(0, (int)r.Data.Player2.Score);

        }

        /// <summary>
        /// Test successful gamestatus while the game is running.
        /// Then play word for player 1 and check the game status.
        /// Then play word for player 2 and check the game status.
        /// Then wait for the gamestate to be completed and check that the 
        /// game status correctly shows all words when the game is completed.
        /// </summary>
        [TestMethod]
        public void TestGameStatus6()
        {
            string[] result = StartBoggleGame(6);
            string gameID = result[0];
            string userToken1 = result[1];
            string userToken2 = result[2];

            Response gameStatusResponse1 = client.DoGetAsync("/games/" + gameID).Result;
            Assert.AreEqual("tester1", gameStatusResponse1.Data.Player1.Nickname.ToString());
            Assert.AreEqual("tester2", gameStatusResponse1.Data.Player2.Nickname.ToString());

            dynamic data1 = new ExpandoObject();
            data1.UserToken = userToken1;
            data1.Word = "sasdd";

            Response playWord1 = client.DoPutAsync(data1, "/games/" + gameID).Result;
            Response gameStatusResponse2 = client.DoGetAsync("/games/" + gameID).Result;
            Assert.AreEqual(-1, (int)gameStatusResponse2.Data.Player1.Score);

            dynamic data2 = new ExpandoObject();
            data2.UserToken = userToken2;
            data2.Word = "fsdfsd";

            Response playWord2 = client.DoPutAsync(data2, "/games/" + gameID).Result;
            Response gameStatusResponse3 = client.DoGetAsync("/games/" + gameID).Result;
            Assert.AreEqual(-1, (int)gameStatusResponse3.Data.Player2.Score);

            Thread.Sleep(7000);

            Response gameStatusResponse4 = client.DoGetAsync("/games/" + gameID).Result;
            Assert.AreEqual("completed", gameStatusResponse4.Data.GameState.ToString());
            Assert.AreEqual("fsdfsd", gameStatusResponse4.Data.Player2.WordsPlayed[0].Word.ToString());
            Assert.AreEqual("sasdd", gameStatusResponse4.Data.Player1.WordsPlayed[0].Word.ToString());
        }

        // TODO still need more GameStatus tests? Or add checks that unexpected things are null to existing?

        /// <summary>
        /// Helper method that starts a Boggle game on the server.
        /// Returns an array containing GameID, Player1's UserToken, and Player2's UserToken.
        /// </summary>
        private string[] StartBoggleGame(int timeLimit)
        {
            ClearPendingGame();

            // First create a user
            dynamic data = new ExpandoObject();
            data.Nickname = "tester1";
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
            data.Nickname = "tester2";
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

        /// <summary>
        /// Ensures that the pending game on the server has 0 players.
        /// </summary>
        private void ClearPendingGame()
        {
            int timeLimit = 5;

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

            if (r.Status == Created)
            {
                return;
            }

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
        }
    }
}
