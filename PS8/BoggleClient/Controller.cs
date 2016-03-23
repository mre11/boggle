using System;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

// TODO maybe we can disable/enable the Cancel button in the start window depending on whether there's a pending request?
// TODO we still need to write the help menu
// TODO testing and error handling
// TODO From assignment: The interface should remain responsive even if a REST request takes a long time to complete.  When a request is active, a Cancel button should become active.  Clicking on this button should gracefully cancel the request.  (Note that the various request methods in C# all have versions that take CancellationTokens as parameters.)
// TODO make sure we have all the comments we need

namespace BoggleClient
{
    /// <summary>
    /// Provides a controller for the Boggle client
    /// </summary>
    public class Controller
    {
        /// <summary>
        /// The window displaying an instance of a game
        /// </summary>
        private IBoggleBoard gameWindow;

        /// <summary>
        /// The window from which a user can request a new game
        /// </summary>
        private StartForm startWindow;

        /// <summary>
        /// The current unique user token
        /// </summary>
        private string userToken;

        /// <summary>
        /// The current unique game ID
        /// </summary>
        private string gameId;

        /// <summary>
        /// Creates a controller for the given game board and start window
        /// </summary>
        public Controller(IBoggleBoard game, StartForm start)
        {
            startWindow = start;
            startWindow.JoinGameEvent += RunGame;
            startWindow.CancelEvent += CancelJoinRequest;

            gameWindow = game;
            gameWindow.PlayWordEvent += PlayWord;
            gameWindow.ExitGameEvent += ExitGame;

            userToken = "";
            gameId = "";
        }

        private async void RunGame()
        {
            // Create a new user
            userToken = await CreateUser(startWindow.PlayerName);

            // Join a game
            gameId = await JoinGame(userToken, startWindow.RequestedDuration);

            // Get the initial game status
            dynamic gameStatus = null;

            while (gameStatus == null || gameStatus.GameState != "active")
            {
                gameStatus = await GameStatus(gameId, false);
            }

            gameWindow.WriteBoardSpaces(gameStatus.Board.ToString());
            gameWindow.Player1Name = gameStatus.Player1.Nickname;
            gameWindow.Player1Score = gameStatus.Player1.Score;
            gameWindow.Player2Name = gameStatus.Player2.Nickname;
            gameWindow.Player2Score = gameStatus.Player2.Score;
            gameWindow.TimeLeft = gameStatus.TimeLeft;

            bool success = userToken != "" && gameId != "";

            if (success)
            {
                startWindow.Hide();
                gameWindow.OpenWindow();

                // TODO use this result to show some kind of results window
                dynamic result = await ContinuousUpdateGameStatus();
            }
            else
            {
                // Display error message telling the user they've entered the wrong Boggle Server.
                // TODO also catch the case that they haven't entered a player name! generally more error handling
                startWindow.DisplayErrorMessage();
            }
        }

        /// <summary>
        /// Continuously requests the game status to update the scores and time left in the view.
        /// Returns the status of the completed game, or the empty string if the game was not completed.
        /// </summary>
        private async Task<dynamic> ContinuousUpdateGameStatus()
        {
            dynamic gameStatus = await GameStatus(gameId, true);

            while (gameStatus.GameState == "active")
            {
                gameWindow.Player1Score = gameStatus.Player1.Score;
                gameWindow.Player2Score = gameStatus.Player2.Score;
                gameWindow.TimeLeft = gameStatus.TimeLeft;

                // TODO how to properly only update once a second? this way freezes up the GUI
                //Thread.Sleep(900);

                gameStatus = await GameStatus(gameId, true);
            }

            gameStatus = await GameStatus(gameId, false);
            gameWindow.Player1Score = gameStatus.Player1.Score;
            gameWindow.Player2Score = gameStatus.Player2.Score;
            gameWindow.TimeLeft = gameStatus.TimeLeft;

            if (gameStatus.GameState == "completed")
            {
                return gameStatus;
            }
            return "";
        }

        /// <summary>
        /// Creates a new user with the given name for the boggle game.  Returns a unique user token.
        /// </summary>
        private async Task<string> CreateUser(string name)
        {
            using (HttpClient client = CreateClient(startWindow.ServerUrl))
            {
                // Create the data for the request
                dynamic data = new ExpandoObject();
                data.Nickname = name;

                // Send POST request
                StringContent content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync("/BoggleService.svc/users", content);

                if (response.IsSuccessStatusCode)
                {
                    // Return the unique user token from the response
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    return result.UserToken;
                }
                return "";
            }
        }

        /// <summary>
        /// Requests the server to join a game using the given user token and specified duration
        /// </summary>
        private async Task<string> JoinGame(string userToken, int duration)
        {
            using (HttpClient client = CreateClient(startWindow.ServerUrl))
            {
                // Create the data for the request
                dynamic data = new ExpandoObject();
                data.UserToken = userToken;
                data.TimeLimit = duration;

                // Send POST request
                StringContent content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync("/BoggleService.svc/games", content);

                if (response.IsSuccessStatusCode)
                {
                    // Return the unique user token from the response
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    return result.GameID;
                }
                return "";
            }
        }

        /// <summary>
        /// Sends a cancel join request
        /// </summary>
        private async void CancelJoinRequest()
        {
            using (HttpClient client = CreateClient(startWindow.ServerUrl))
            {
                // Create the data for the request
                dynamic data = new ExpandoObject();
                data.UserToken = userToken;

                // Send PUT request
                StringContent content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                await client.PutAsync("/BoggleService.svc/games", content);
            }
        }

        /// <summary>
        /// Sends a play word request using the given word
        /// </summary>
        private async void PlayWord(string word)
        {
            using (HttpClient client = CreateClient(startWindow.ServerUrl))
            {
                // Create the data for the request
                dynamic data = new ExpandoObject();
                data.UserToken = userToken;
                data.Word = word;

                // Send PUT request
                StringContent content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                HttpResponseMessage response = await client.PutAsync("/BoggleService.svc/games/" + gameId, content);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(response.StatusCode.ToString());
                }
            }
        }

        /// <summary>
        /// Gets the game status from the boggle server for a specific gameID. 
        /// </summary>
        private async Task<dynamic> GameStatus(string gameId, bool brief)
        {
            using (HttpClient client = CreateClient(startWindow.ServerUrl))
            {
                // Get the game status for the specified gameID                
                HttpResponseMessage response = await client.GetAsync("/BoggleService.svc/games/" + gameId + (brief ? "?Brief:yes":""));

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject(responseContent);
                }
                return null;
            }
        }

        /// <summary>
        /// If the game window is exited, hides the window instead of closing
        /// and brings up the start window for a new game.
        /// </summary>
        private void ExitGame(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                gameWindow.HideWindow();
                startWindow.Show();
            }
        }

        /// <summary>
        /// Returns a client with the given url as its base address
        /// </summary>
        private HttpClient CreateClient(string url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client;
        }
    }
}
