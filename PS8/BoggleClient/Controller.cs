using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BoggleClient
{
    public class Controller
    {
        private IBoggleBoard window;

        public Controller(IBoggleBoard window)
        {
            this.window = window;
            window.JoinGameEvent += CreateClient;
            window.PlayWordEvent += PlayWord;
        }

        private void JoinGame(string url, string playerName, int timeLeft)
        {
        }
        private void PlayWord(string word)
        {
            if (String.IsNullOrEmpty(word.Trim()))
            {
                // Respond with code 403 (Forbidden)
            }
            //else if ()
            //{
            //    // (Server game id || UserToken) is missing or invalid //)
            //}
        }

        private void CreateClient(string url, string playerName, int timeLeft)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Clear();

            throw new NotImplementedException();
        }

        private string CreateUser(string name)
        {
            throw new NotImplementedException();
        }
    }
}
