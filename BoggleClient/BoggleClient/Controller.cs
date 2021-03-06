﻿// Created for CS3500, Spring 2016
// Morgan Empey (U0634576), Braden Klunker (U0725294)

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
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
        /// The current unique user token
        /// </summary>
        private string userToken;

        /// <summary>
        /// The current unique game ID
        /// </summary>
        private string gameId;

        /// <summary>
        /// Timer
        /// </summary>
        private System.Timers.Timer updateTimer;

        /// <summary>
        /// True if the game board hasn't been initialized yet
        /// </summary>
        private bool initializeGameBoard;

        /// <summary>
        /// Provides a source for a cancellation token
        /// </summary>
        private CancellationTokenSource cts;

        /// <summary>
        /// Creates a controller for the given game board and start window
        /// </summary>
        public Controller(IBoggleBoard game, StartForm start)
        {
            startWindow = start;
            startWindow.JoinGameEvent += StartGame;
            startWindow.CancelEvent += CancelJoinRequest;

            gameWindow = game;
            gameWindow.PlayWordEvent += PlayWordHandler;
            gameWindow.ExitGameEvent += ExitGame;

            userToken = "";
            gameId = "";
            initializeGameBoard = true;
            updateTimer = new System.Timers.Timer(1000);
            cts = new CancellationTokenSource();
        }

        /// <summary>
        /// Sets up and runs a single game
        /// </summary>
        private async void StartGame()
        {
            try
            {
                // Create a new user
                userToken = await CreateUser(startWindow.PlayerName, cts.Token);
                if (userToken == null)
                {
                    throw new HttpRequestException();
                }

                // Join a game
                startWindow.JoiningGame = true;
                
                gameId = await JoinGame(userToken, startWindow.RequestedDuration, cts.Token);

                if (gameId == null)
                {
                    throw new HttpRequestException();
                }
                
                // Start timer to update the game view periodically
                updateTimer.Elapsed += UpdateGameStatus;
                updateTimer.AutoReset = true;
                updateTimer.Start();
            }
            catch (Exception)
            {
                startWindow.DisplayErrorMessage();
            }
        }

        /// <summary>
        /// Requests the game status and uses it to update the view
        /// </summary>
        private async void UpdateGameStatus(object sender, System.Timers.ElapsedEventArgs e)
        {
            dynamic gameStatus = null;
            try
            {
                gameStatus = await GameStatus(gameId, true, cts.Token);
            }
            catch (Exception) { }

            if (gameStatus == null)
            {
                updateTimer.Stop();                
                initializeGameBoard = true;
                startWindow.Invoke((Action)(() =>
                {
                    gameWindow.HideWindow();
                }));
                return;
            }
            else if (gameStatus.GameState == "completed")
            {
                updateTimer.Stop();
                startWindow.Invoke((Action)(() =>
                {
                    gameWindow.EnterButtonEnabled = false;
                    gameWindow.EnterBoxEnabled = false;                    
                }));

                initializeGameBoard = true;                
                ShowResults(gameStatus);

            }
            else if (gameStatus.GameState == "active")
            {
                startWindow.Invoke((Action)(() => 
                {
                    if (initializeGameBoard)
                    {
                        initializeGameBoard = false;
                        startWindow.JoiningGame = false;
                        startWindow.Hide();
                        gameWindow.ShowWindow();
                        gameWindow.WriteBoardSpaces(gameStatus.Board.ToString());
                        gameWindow.Player1Name = gameStatus.Player1.Nickname;
                        gameWindow.Player2Name = gameStatus.Player2.Nickname;
                    }      
                                  
                    gameWindow.Player1Score = gameStatus.Player1.Score;                    
                    gameWindow.Player2Score = gameStatus.Player2.Score;
                    gameWindow.TimeLeft = gameStatus.TimeLeft;
                }));
            }
        }

        /// <summary>
        /// Displays the results of a completed game given a GameStatus response.
        /// </summary>
        private void ShowResults(dynamic gameStatus)
        {
            var player1Words = new List<string>();
            var player1Scores = new List<string>();
            var player2Words = new List<string>();
            var player2Scores = new List<string>();

            // Store the words and scores in lists
            foreach (dynamic wordScorePair in gameStatus.Player1.WordsPlayed)
            {
                string word = wordScorePair.Word;
                string score = wordScorePair.Score;
                player1Words.Add(word);
                player1Scores.Add(score);
            }

            foreach (dynamic wordScorePair in gameStatus.Player2.WordsPlayed)
            {
                string word = wordScorePair.Word;
                string score = wordScorePair.Score;
                player2Words.Add(word);
                player2Scores.Add(score);
            }

            // Write out a string containing the results
            var builder = new StringBuilder();
            builder.AppendLine("Player 1:\t" + gameStatus.Player1.Nickname);
            for (int i = 0; i < player1Words.Count; i++)
            {
                builder.AppendLine(player1Words[i] + "\t" + player1Scores[i]);
            }
            builder.AppendLine();
            builder.AppendLine("Player 2:\t" + gameStatus.Player2.Nickname);
            for (int i = 0; i < player2Words.Count; i++)
            {
                builder.AppendLine(player2Words[i] + "\t" + player2Scores[i]);
            }
            
            MessageBox.Show(builder.ToString(), "Results");
        }

        /// <summary>
        /// Creates a new user with the given name for the boggle game.  Returns a unique user token.
        /// </summary>
        private async Task<string> CreateUser(string name, CancellationToken token)
        {
            using (HttpClient client = CreateClient(startWindow.ServerUrl))
            {
                // Create the data for the request
                dynamic data = new ExpandoObject();
                data.Nickname = name;

                // Send POST request
                StringContent content = new StringContent(JsonConvert.SerializeObject(data));
                content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");
                HttpResponseMessage response = await client.PostAsync("/BoggleService.svc/users", content, token);

                if (response.IsSuccessStatusCode)
                {
                    // Return the unique user token from the response
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    return result.UserToken;
                }
                return null;
            }
        }

        /// <summary>
        /// Requests the server to join a game using the given user token and specified duration
        /// </summary>
        private async Task<string> JoinGame(string userToken, int duration, CancellationToken token)
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
                HttpResponseMessage response = await client.PostAsync("/BoggleService.svc/games", content, token);

                if (response.IsSuccessStatusCode)
                {
                    // Return the unique user token from the response
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);
                    return result.GameID;
                }
                return null;
            }
        }

        /// <summary>
        /// Sends a cancel join request
        /// </summary>
        private async void CancelJoinRequest()
        {
            cts.Cancel();
            cts = new CancellationTokenSource();

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
            startWindow.JoiningGame = false;
        }

        /// <summary>
        /// Sends a play word request using the given word
        /// </summary>
        private async void PlayWord(string word, CancellationToken token)
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
                HttpResponseMessage response = await client.PutAsync("/BoggleService.svc/games/" + gameId, content, token);

                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show(response.StatusCode.ToString());
                }
            }
        }

        /// <summary>
        /// Handles the PlayWord event
        /// </summary>
        private void PlayWordHandler(string word)
        {
            PlayWord(word, cts.Token);
        }

        /// <summary>
        /// Gets the game status from the boggle server for a specific gameID. 
        /// </summary>
        private async Task<dynamic> GameStatus(string gameId, bool brief, CancellationToken token)
        {
            using (HttpClient client = CreateClient(startWindow.ServerUrl))
            {
                // Get the game status for the specified gameID                
                HttpResponseMessage response = await client.GetAsync("/BoggleService.svc/games/" + gameId + (brief ? "?Brief:yes":""), token);

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
                cts.Cancel();
                cts = new CancellationTokenSource();
                gameWindow.EnterBoxEnabled = true;
                gameWindow.EnterButtonEnabled = true;
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
