using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BoggleClient
{
    class Controller
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
        /// The URL of the Boggle server
        /// </summary>
        private string serverUrl;

        /// <summary>
        /// The player name for the game instance
        /// </summary>
        private string playerName;

        /// <summary>
        /// The user-requested Boggle game duration
        /// </summary>
        private int requestedDuration;

        public Controller(IBoggleBoard game, StartForm start)
        {
            //  TODO Maybe the constructor should only take "start" as a parameter
            // then the Controller can create an IBoggleBoard later once a game is joined?
            startWindow = start;
            startWindow.JoinGameEvent += JoinGame;
            startWindow.CancelEvent += CancelGameSearch;

            gameWindow = game;            
            gameWindow.PlayWordEvent += PlayWord;
            gameWindow.ExitGameEvent += ExitGame;            
        }

        private void JoinGame()
        {
            // TODO these may not need to be member variables
            serverUrl = startWindow.ServerUrl;
            playerName = startWindow.PlayerName;
            requestedDuration = startWindow.RequestedDuration;
        }

        private void CancelGameSearch()
        {
            throw new NotImplementedException();
        }

        private void PlayWord(string word)
        {
            throw new NotImplementedException();
        }

        private void ExitGame()
        {            
            gameWindow.CloseWindow();
            startWindow.Show();
            throw new NotImplementedException();
        }

        private HttpClient CreateClient(string url)
        {
            throw new NotImplementedException();
        }

        private string CreateUser(string name)
        {
            throw new NotImplementedException();
        }        
    }
}
