using System;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

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
        /// Creates a controller for the given game board and start window
        /// </summary>
        public Controller(IBoggleBoard game, StartForm start)
        {
            startWindow = start;
            startWindow.JoinGameEvent += JoinGame;
            startWindow.CancelEvent += CancelGameSearch;

            gameWindow = game;            
            gameWindow.PlayWordEvent += PlayWord;
            gameWindow.ExitGameEvent += ExitGame;            
        }

        private async void JoinGame()
        {
            string userToken = await CreateUser(startWindow.PlayerName);

            HttpClient client = CreateClient(startWindow.ServerUrl);


            // Try connecting to the boggle server
            bool success = true;

            if (success)
            {
                // TODO populate the game window using the game status
                startWindow.Hide();
                gameWindow.OpenWindow();
            }
            else
            {
                // Display error message telling the user they've entered the wrong Boggle Server.
                // TODO also catch the case that they haven't entered a player name!
                startWindow.DisplayErrorMessage();
            }            
        }

        private async void CancelGameSearch()
        {
            // TODO: send a cancel join request to the server, don't do anything else
        }

        private void PlayWord(string word)
        {
            throw new NotImplementedException();
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
                else
                {
                    return "";
                }
            }            
        }
    }
}
