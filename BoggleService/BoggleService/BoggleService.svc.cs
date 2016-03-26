using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class BoggleService : IBoggleService
    {
        /// <summary>
        /// The most recent call to SetStatus determines the response code used when
        /// an http response is sent.
        /// </summary>
        /// <param name="status"></param>
        private static void SetStatus(HttpStatusCode status)
        {
            WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        }

        /// <summary>
        /// Returns a Stream version of index.html.
        /// </summary>
        /// <returns></returns>
        public Stream API()
        {
            SetStatus(OK);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "index.html");
        }

        public void CancelJoin(string userToken)
        {
            throw new NotImplementedException();
        }

        public string CreateUser(string nickname)
        {
            throw new NotImplementedException();
        }

        public IList<string> GameStatus(bool brief, string gameID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Demo.  You can delete this.
        /// </summary>
        public int GetFirst(IList<int> list)
        {
            SetStatus(OK);
            return list[0];
        }

        public string JoinGame(string userToken, int timeLimit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Demo.  You can delete this.
        /// </summary>
        /// <returns></returns>
        public IList<int> Numbers(string n)
        {
            int index;
            if (!Int32.TryParse(n, out index) || index < 0)
            {
                SetStatus(Forbidden);
                return null;
            }
            else
            {
                List<int> list = new List<int>();
                for (int i = 0; i < index; i++)
                {
                    list.Add(i);
                }
                SetStatus(OK);
                return list;
            }
        }

        public void PlayWord(string gameID, string userToken, string word)
        {
            throw new NotImplementedException();
        }
    }
}
