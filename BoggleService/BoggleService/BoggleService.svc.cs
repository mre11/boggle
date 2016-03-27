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
        /// <summary>
        /// Keeps track of the unique Users on the service.
        /// The key is the User's UserToken.
        /// </summary>
        private readonly static Dictionary<string, User> users = new Dictionary<string, User>();

        /// <summary>
        /// Keeps track of the unique BoggleGames on the service.
        /// The key is the BoggleGame's GameID.
        /// </summary>
        private readonly static Dictionary<string, BoggleGame> games = new Dictionary<string, BoggleGame>();

        /// <summary>
        /// The pending game has a GameID.
        /// If one player belongs to the pending game, it also has a UserID and a requested time limit.
        /// There is always exactly one pending game.
        /// </summary>
        private static BoggleGame pendingGame;

        /// <summary>
        /// Provides a way to lock the data representation
        /// </summary>
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
        public Stream API()
        {
            SetStatus(OK);
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "index.html");
        }

        /// <summary>
        /// Creates a new user. 
        /// 
        /// If Nickname is null, or is empty when trimmed, responds with status 403 (Forbidden).
        /// 
        /// Otherwise, creates a new user with a unique UserToken and the trimmed Nickname.The returned UserToken 
        /// should be used to identify the user in subsequent requests.Responds with status 201 (Created). 
        /// </summary>
        public User CreateUser(User requestedUser)
        {
            lock(sync)
            {
                if (requestedUser.Nickname == null || requestedUser.Nickname.Trim() == "")
                {
                    SetStatus(Forbidden);
                    return null;
                }

                string userToken = Guid.NewGuid().ToString();
                requestedUser.UserToken = userToken;

                users.Add(requestedUser.UserToken, requestedUser);

                SetStatus(Created);
                requestedUser.Nickname = null;                
                return requestedUser;
            }

        }

        public BoggleGame JoinGame(BoggleGame game)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Cancel a pending request to join a game.
        /// 
        /// If UserToken is invalid or is not a player in the pending game, responds with status 403 (Forbidden).
        /// 
        /// Otherwise, removes UserToken from the pending game and responds with status 200 (OK).
        /// </summary>
        public void CancelJoin(User user)
        {
            throw new NotImplementedException();
        }

        public BoggleWord PlayWord(string gameID, BoggleWord word)
        {
            throw new NotImplementedException();
        }

        public BoggleGame GameStatus(string brief, string gameID)
        {
            lock (sync)
            {
                if (!games.ContainsKey(gameID))
                {
                    SetStatus(Forbidden);
                    return null;
                }

                SetStatus(OK);

                dynamic status = new ExpandoObject();
                if (brief == "yes")
                {
                    status.GameState = "active";
                    BoggleGame temp;
                    games.TryGetValue(gameID, out temp);
                    status.Board = temp.ToString();
                    //status.TimeLeft = 

                }
                else
                {

                }
                return status;
            }
        }
    }
}
