using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            try
            {

                lock (sync)
                {
                    InitializePendingGame();

                    if (requestedUser.Nickname == null || requestedUser.Nickname.Trim() == "")
                    {
                        SetStatus(Forbidden);
                        return null;
                    }

                    string userToken = Guid.NewGuid().ToString();
                    requestedUser.UserToken = userToken;
                    requestedUser.Score = 0;

                    users.Add(requestedUser.UserToken, requestedUser);

                    var response = new User();
                    response.UserToken = userToken;
                    //response.Score = null;

                    SetStatus(Created);

                    return response;
                }
            }
            catch (Exception)
            {
                SetStatus(InternalServerError);
                return null;
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
        /// active game's GameID (which should be the same as the old pending game's GameID). Responds with 
        /// status 201 (Created).
        /// 
        /// Otherwise, adds UserToken as the first player of the pending game, and the TimeLimit as the pending 
        /// game's requested time limit. Returns the pending game's GameID. Responds with status 202 (Accepted).
        /// </summary>
        public BoggleGame JoinGame(JoinGameRequest requestBody)
        {
            try
            {
                lock (sync)
                {
                    InitializePendingGame();

                    if (requestBody.UserToken == null || requestBody.UserToken == ""
                        || requestBody.TimeLimit < 5 || requestBody.TimeLimit > 120)
                    {
                        SetStatus(Forbidden);
                        return null;
                    }

                    BoggleGame pendingGame;
                    if (games.TryGetValue(pendingGameID, out pendingGame))
                    {
                        // Create User newPlayer to act as reference to the user in the users list.
                        User newPlayer;

                        // Haven't created the user yet.
                        if (!users.TryGetValue(requestBody.UserToken, out newPlayer))
                        {
                            SetStatus(Conflict);
                            return null;
                        }

                        if (pendingGame.Player1 == null) // pending game has 0 players
                        {
                            pendingGame.Player1 = newPlayer;
                            pendingGame.Player1.WordsPlayed = new List<BoggleWord>();
                            pendingGame.TimeLimit = requestBody.TimeLimit;
                            SetStatus(Accepted);

                            // Compose the response. It should only contain the GameID.
                            var response = new BoggleGame();
                            response.GameID = pendingGameID;
                            return response;
                        }
                        else if (pendingGame.Player1.UserToken == requestBody.UserToken) // requested user token is already in the pending game
                        {
                            SetStatus(Conflict);
                            return null;
                        }
                        else // pending game has 1 player
                        {
                            pendingGame.Player2 = newPlayer;
                            pendingGame.Player2.WordsPlayed = new List<BoggleWord>();
                            pendingGame.TimeLimit = (pendingGame.TimeLimit + requestBody.TimeLimit) / 2;
                            pendingGame.GameState = "active";
                            pendingGame.TimeStarted = Environment.TickCount;

                            // Compose the response. It should only contain the GameID.
                            var response = new BoggleGame();
                            response.GameID = pendingGameID;

                            // Create the next pending game
                            pendingGameID++;
                            games.Add(pendingGameID, new BoggleGame(pendingGameID));

                            SetStatus(Created);
                            return response;
                        }
                    }

                    throw new Exception();
                }
            }
            catch (Exception)
            {
                SetStatus(InternalServerError);
                return null;
            }
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
            try
            {
                lock (sync)
                {
                    InitializePendingGame();

                    BoggleGame pendingGame;
                    string pendingUserToken = "";
                    if (games.TryGetValue(pendingGameID, out pendingGame) && pendingGame.Player1 != null)
                    {
                        if (pendingGame.Player1.UserToken != null)
                        {
                            pendingUserToken = pendingGame.Player1.UserToken;
                        }
                    }

                    User existingUser;
                    if (user.UserToken == null || !users.TryGetValue(user.UserToken, out existingUser)
                        || user.UserToken != pendingUserToken)
                    {
                        SetStatus(Forbidden);
                        return;
                    }

                    pendingGame.Player1 = null;
                    SetStatus(OK);
                }
            }
            catch (Exception)
            {
                SetStatus(InternalServerError);
            }
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
            try
            {
                lock (sync)
                {
                    InitializePendingGame();

                    // Word or UserToken is invalid
                    User user;
                    if (word.Word == null || word.UserToken == null || word.Word.Trim() == ""
                        || !users.TryGetValue(word.UserToken, out user))
                    {
                        SetStatus(Forbidden);
                        return null;
                    }

                    // GameID is invalid or we couldn't find the game
                    int intGameID;
                    BoggleGame game;
                    if (!int.TryParse(gameID, out intGameID) || !games.TryGetValue(intGameID, out game))
                    {
                        SetStatus(Forbidden);
                        return null;
                    }
                    
                    // UserToken is not a player in this game
                    if (game.Player1.UserToken != word.UserToken && game.Player2.UserToken != word.UserToken)
                    {
                        SetStatus(Forbidden);
                        return null;
                    }

                    if (game.GameState != "active")
                    {
                        SetStatus(Conflict);
                        return null;
                    }

                    var playedWord = word.Word;
                    playedWord = playedWord.Trim();
                    playedWord = playedWord.ToLower();

                    var playedBoggleWord = new BoggleWord();
                    playedBoggleWord.Word = playedWord;
                    playedBoggleWord.UserToken = user.UserToken;

                    int wordScore = 0;

                    if (game.Board.CanBeFormed(playedWord))
                    {
                        if (game.wordsPlayed.Contains(playedWord))
                            wordScore = 0;
                        else if (playedWord.Length > 7)
                            wordScore = 11;
                        else if (playedWord.Length > 6)
                            wordScore = 5;
                        else if (playedWord.Length > 5)
                            wordScore = 3;
                        else if (playedWord.Length > 4)
                            wordScore = 2;
                        else if (playedWord.Length > 2)
                            wordScore = 1;
                    }
                    else
                    {
                        wordScore = -1;
                    }

                    // TODO: need to add up score and save it in the user.

                    //game.wordsPlayed.Add(playedWord);

                    var result = new BoggleWord();  // this will be returned and hold only the score
                    playedBoggleWord.Score = result.Score = wordScore;
                    
                    // Initialize user.WordsPlayed if it is null.
                    if(user.WordsPlayed == null)
                    {
                        user.WordsPlayed = new List<BoggleWord>();
                    }

                    user.WordsPlayed.Add(playedBoggleWord);
                    user.Score += wordScore;

                    return result;
                }
            }
            catch (Exception)
            {
                SetStatus(InternalServerError);
                return null;
            }
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
        public BoggleGame GameStatus(string gameID, string brief)
        {
            // TODO need to make sure output is supressed where necessary (i.e. null out some fields before returning)
            try
            {
                lock (sync)
                {
                    InitializePendingGame();

                    // Invalid gameID or the game doesn't exist
                    int intGameID;
                    BoggleGame currentGame;

                    if (!int.TryParse(gameID, out intGameID) || !games.TryGetValue(intGameID, out currentGame))
                    {
                        SetStatus(Forbidden);
                        return null;
                    }

                    SetStatus(OK);
                    currentGame.timeLeft = currentGame.TimeLeft;

                    if (currentGame.GameState == null || currentGame.GameState == "pending" || (currentGame.Player1 == null || currentGame.Player2 == null)) 
                    {
                        var response = new BoggleGame();
                        response.GameState = "pending";
                        return response;
                    }
                    else if (currentGame.timeLeft == null || currentGame.timeLeft == 0)
                    {
                        currentGame.GameState = "completed";
                    }

                    if (brief != null && brief == "yes")
                    {
                        var briefGameStatus = new BoggleGame();
                        briefGameStatus.GameState = currentGame.GameState;
                        briefGameStatus.timeLeft = currentGame.timeLeft;                                             
                        briefGameStatus.Player1 = new User();
                        briefGameStatus.Player2 = new User();
                        briefGameStatus.Player1.Score = currentGame.Player1.Score;
                        briefGameStatus.Player2.Score = currentGame.Player2.Score;

                        return briefGameStatus;
                    }
                    else
                    {
                        var regGameStatus = new BoggleGame();

                        regGameStatus.GameState = currentGame.GameState;
                        regGameStatus.Board = currentGame.Board;
                        regGameStatus.board = regGameStatus.Board.ToString();

                        // TimeLimit and TimeStarted are needed for correct computation of timeLeft
                        regGameStatus.TimeLimit = currentGame.TimeLimit;
                        regGameStatus.TimeStarted = currentGame.TimeStarted;

                        // Need to fix timeleft
                        regGameStatus.timeLeft = currentGame.timeLeft;

                        regGameStatus.Player1 = new User();
                        regGameStatus.Player1.Nickname = currentGame.Player1.Nickname;
                        regGameStatus.Player1.Score = currentGame.Player1.Score;

                        regGameStatus.Player2 = new User();
                        regGameStatus.Player2.Nickname = currentGame.Player2.Nickname;
                        regGameStatus.Player2.Score = currentGame.Player2.Score;

                        if (currentGame.GameState == "completed")
                        {
                            regGameStatus.Player1.WordsPlayed = new List<BoggleWord>();

                            foreach (BoggleWord word in currentGame.Player1.WordsPlayed)
                            {
                                BoggleWord formattedWord = new BoggleWord();
                                formattedWord.Word = word.Word;
                                formattedWord.Score = word.Score;
                                regGameStatus.Player1.WordsPlayed.Add(formattedWord);
                            }

                            regGameStatus.Player2.WordsPlayed = new List<BoggleWord>();

                            foreach (BoggleWord word in currentGame.Player2.WordsPlayed)
                            {
                                BoggleWord formattedWord = new BoggleWord();
                                formattedWord.Word = word.Word;
                                formattedWord.Score = word.Score;
                                regGameStatus.Player2.WordsPlayed.Add(formattedWord);
                            }
                        }
                        return regGameStatus;
                    }
                }
            }
            catch (Exception)
            {
                SetStatus(InternalServerError);
                return null;
            }
        }

        /// <summary>
        /// If the games record is empty, add a game as the first pending game.
        /// </summary>
        private void InitializePendingGame()
        {
            if (games.Count == 0)
            {
                games.Add(pendingGameID, new BoggleGame(pendingGameID));
            }
        }
    }
}
