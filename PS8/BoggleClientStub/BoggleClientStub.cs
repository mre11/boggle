using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoggleClient;

namespace BoggleClientControllerTester
{
    class BoggleClientStub : IBoggleBoard
    {
        public string BoggleServerURL
        {
            get; set;
        }

        public string Player1Name
        {
            get; set;
        }

        public string Player1Score
        {
            get; set;
        }

        public string Player2Name
        {
            get; set;
        }

        public string Player2Score
        {
            get; set;
        }

        public event Action ExitGameEvent;
        public event Action<string, string, int> JoinGameEvent;
        public event Action<string> PlayWordEvent;
    }
}
