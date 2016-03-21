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
        private IBoggleBoard window;

        public Controller(IBoggleBoard window)
        {
            window.JoinGameEvent += JoinGame;
            window.PlayWordEvent += PlayWord;
            
        }

        private void JoinGame(string url, string playerName, int timeLeft)
        {

        }
        private void PlayWord(string word)
        {

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
