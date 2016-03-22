using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

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

        private void CancelGameSearch()
        {
            // TODO: I think we need to some how implement this Asyn.
            // maybe this isn't async but the join is?
            // also we don't want this to close the window, just cancel the join game search...somehow...
            // startWindow.Close();
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
