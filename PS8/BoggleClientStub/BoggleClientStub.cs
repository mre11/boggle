using System;
using BoggleClient;

namespace BoggleClientControllerTester
{
    class BoggleClientStub : IBoggleBoard
    {
        public string Player1Name
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Player1Score
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Player2Name
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string Player2Score
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string TimeLeft
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public event Action<System.Windows.Forms.FormClosingEventArgs> ExitGameEvent;
        public event Action<string> PlayWordEvent;

        public void HideWindow()
        {
            throw new NotImplementedException();
        }

        public void ShowWindow()
        {
            throw new NotImplementedException();
        }

        public void WriteBoardSpaces(string board)
        {
            throw new NotImplementedException();
        }
    }
}
