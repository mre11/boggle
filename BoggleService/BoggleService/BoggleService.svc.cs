using Newtonsoft.Json;
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
        private readonly static Dictionary<int, BoggleGame> games = new Dictionary<int, BoggleGame>();

        /// <summary>
        /// The pending game has a GameID.
        /// If one player belongs to the pending game, it also has a UserID and a requested time limit.
        /// There is always exactly one pending game.
        /// </summary>
        private static int pendingGameID = 1;

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

        /// <summary>
        /// Join a game.
        /// 
        /// If UserToken is invalid, TimeLimit<5, or TimeLimit> 120, responds with status 403 (Forbidden).
        /// 
        /// Otherwise, if UserToken is already a player in the pending game, responds with status 409 (Conflict).
        /// 
        /// Otherwise, if there is already one player in the pending game, adds UserToken as the second player.
        /// The pending game becomes active and a new pending game with no players is created.The active game's 
        /// time limit is the integer average of the time limits requested by the two players. Returns the new 
        /// active game's GameID(which should be the same as the old pending game's GameID). Responds with 
        /// status 201 (Created).
        /// 
        /// Otherwise, adds UserToken as the first player of the pending game, and the TimeLimit as the pending 
        /// game's requested time limit. Returns the pending game's GameID. Responds with status 202 (Accepted).
        /// </summary>
        public BoggleGame JoinGame(JoinGameRequest requestBody)
        {          
            // This is the usage I'm thinking we'll want to keep track of the pending game
            var tempGame = new BoggleGame(pendingGameID++);

            if((requestBody.UserToken == "" || requestBody.UserToken == null) || (requestBody.TimeLimit < 5 || requestBody.TimeLimit > 120))
            {
                SetStatus(Forbidden);
                return null;
            }

            if(users.ContainsKey(requestBody.UserToken))
            {
                SetStatus(Conflict);
                return null;
            }
            else
            {

            }


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

        /// <summary>
        /// Play a word in a game.
        /// 
        /// If Word is null or empty when trimmed, or if GameID or UserToken is missing or invalid, or if 
        /// UserToken is not a player in the game identified by GameID, responds with response code 403 (Forbidden).
        /// 
        /// Otherwise, if the game state is anything other than "active", responds with response code 409 (Conflict).
        /// 
        /// Otherwise, records the trimmed Word as being played by UserToken in the game identified by GameID. Returns
        /// the score for Word in the context of the game(e.g. if Word has been played before the score is zero). 
        /// Responds with status 200 (OK). Note: The word is not case sensitive.
        /// </summary>
        public BoggleWord PlayWord(string gameID, BoggleWord word)
        {
            lock (sync)
            {
                int intGameID;
                // Trim word so we only have to trim it once.
                word.Word = word.Word.Trim();

                // TODO: usertoken not a player in the game identified by gameID
                if (!int.TryParse(gameID, out intGameID) || (word.Word == "" || word.Word == null))
                {
                    SetStatus(Forbidden);
                    return null;
                }

                // Get boggle game out of dictionary
                BoggleGame game;

                // Game ID was invalid
                if(games.TryGetValue(intGameID, out game))
                {
                    SetStatus(Forbidden);
                    return null;
                }

                // Not active games will respond with code 403 Forbidden
                if (game.GameState != "active")
                {
                    SetStatus(Conflict);
                    return null;
                } 
            }

            throw new NotImplementedException();
        }


        /// <summary>
        /// Get game status information.
        /// 
        /// If GameID is invalid, responds with status 403 (Forbidden).
        /// 
        /// Otherwise, returns information about the game named by GameID as illustrated below.Note that the information
        /// returned depends on whether "Brief=yes" was included as a parameter as well as on the state of the game. 
        /// Responds with status code 200 (OK). Note: The Board and Words are not case sensitive.
        /// </summary>
        public BoggleGame GameStatus(string brief, string gameID)
        {       
            int intGameID;

            // Forbidden is returned if invalid gameID or no games witht that gameID are currently going on.
            if (!int.TryParse(gameID, out intGameID) || !games.ContainsKey(intGameID))
            {
                SetStatus(Forbidden);
                return null;
            }

            SetStatus(OK);

            //dynamic status = new ExpandoObject();

            BoggleGame temp;

            games.TryGetValue(intGameID, out temp);

            BoggleGame h = new BoggleGame(temp);

            if(temp.GameState == "pending")
            {
                return temp;
            }

            if (brief == "yes")
            {
                // Need
                // gamestate
                // timeleft
                // player1
                // score
                // player2
                // score
                h.GameID = 0;
                h.TimeLimit = 0;
                h.Player1.Nickname = null;
                h.Player1.WordsPlayed = null;
                h.Player2.Nickname = null;
                h.Player2.WordsPlayed = null;
                return h;
            }
            else
            {
                // Need
                // gamestate
                // board
                // timelimit
                // timeleft
                // player1
                // nickname
                // score
                // wordsplayed
                // word
                return temp;
            }
        }


    }
}
