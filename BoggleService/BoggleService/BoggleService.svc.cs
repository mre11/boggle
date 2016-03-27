using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    public class BoggleService : IBoggleService
    {
        private readonly static Dictionary<string, string> tokens = new Dictionary<string, string>();
        private readonly static Dictionary<string, BoggleBoard> boards = new Dictionary<string, BoggleBoard>();
        private readonly static object sync = new object();
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

        public string CreateUser(Player newPlayer)
        {
            lock(sync)
            {
                if (newPlayer.nickname == null || newPlayer.nickname.Trim() == "")
                {
                    SetStatus(Forbidden);
                    return null;
                }

                SetStatus(Created);
                //dynamic response = new ExpandoObject();
                //UserToken = Guid.NewGuid().ToString();
                //return response;

                string userToken = Guid.NewGuid().ToString();
                return userToken;

            }

        }

        public dynamic GameStatus(bool brief, string gameID)
        {
            lock (sync)
            {
                if (!boards.ContainsKey(gameID))
                {
                    SetStatus(Forbidden);
                    return null;
                }

                SetStatus(OK);

                dynamic status = new ExpandoObject();
                if (brief)
                {
                    status.GameState = "active";
                    BoggleBoard temp;
                    boards.TryGetValue(gameID, out temp);
                    status.Board = temp.ToString();
                    //status.TimeLeft = 

                }
                else
                {

                }
                return status; 
            }
        }

        /// <summary>
        /// Demo.  You can delete this.
        /// </summary>
        public int GetFirst(IList<int> list)
        {
            SetStatus(OK);
            return list[0];
        }

        public string JoinGame(BoggleGame game)
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

        public int PlayWord(string gameID, string userToken, string word)
        {
            throw new NotImplementedException();
        }
    }
}
