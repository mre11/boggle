// Braden Klunker, Morgan Empey, CS3500

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Web;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    /// <summary>
    /// Implements a RESTful service for a Boggle game
    /// </summary>
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
        public UserResponse CreateUser(User requestedUser)
        {
            try
            {
                // Lock this code so aliasing doesn't occur.
                lock (sync)
                {
                    InitializePendingGame();

                    // Error check the request.
                    if (requestedUser.Nickname == null || requestedUser.Nickname.Trim() == "")
                    {
                        SetStatus(Forbidden);
                        return null;
                    }

                    // Create new userToken for the player.
                    string userToken = Guid.NewGuid().ToString();
                    requestedUser.UserToken = userToken;
                    requestedUser.Score = 0;

                    // Store this users info.
                    users.Add(requestedUser.UserToken, requestedUser);

                    // Create formatted response to send back. 
                    var response = new UserResponse();
                    response.UserToken = userToken;

                    // Successfully created a new user.
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
        public BoggleGameResponse JoinGame(JoinGameRequest requestBody)
        {
            try
            {
                lock (sync)
                {
                    InitializePendingGame();

                    // Make sure game is >= 5 and <= 120 and the user token is valid, otherwise setStatus to forbidden.
                    if (requestBody.UserToken == null || requestBody.UserToken == ""
                        || requestBody.TimeLimit < 5 || requestBody.TimeLimit > 120)
                    {
                        SetStatus(Forbidden);
                        return null;
                    }

                    // Retrieve the game from the games list with the gameID.
                    BoggleGame pendingGame;
                    if (games.TryGetValue(pendingGameID, out pendingGame))
                    {
                        // Get the user, or return if the user is not found
                        User newPlayer;
                        if (!users.TryGetValue(requestBody.UserToken, out newPlayer))
                        {
                            SetStatus(Forbidden);
                            return null;
                        }

                        // Create formatted response to send back.
                        var response = new BoggleGameResponse();
                        response.GameID = pendingGameID;

                        if (pendingGame.Player1 == null) // pending game has 0 players
                        {
                            // Link the pendingGames player1 with the user from the users list.
                            pendingGame.Player1 = newPlayer;
                            pendingGame.TimeLimit = requestBody.TimeLimit;
                            SetStatus(Accepted);
                        }
                        else if (pendingGame.Player1.UserToken == requestBody.UserToken) // requested user token is already in the pending game
                        {
                            SetStatus(Conflict);
                            return null;
                        }
                        else // pending game has 1 player
                        {
                            // Link the pendingGames player2 with the user from the users list.
                            pendingGame.Player2 = newPlayer;
                            pendingGame.TimeLimit = (pendingGame.TimeLimit + requestBody.TimeLimit) / 2;

                            // Start the game with the average time limit from both users and record the time this game starts.
                            pendingGame.GameState = "active";
                            pendingGame.TimeStarted = Environment.TickCount;

                            // Create the next pending game
                            pendingGameID++;
                            games.Add(pendingGameID, new BoggleGame(pendingGameID));

                            SetStatus(Created);
                        }

                        return response;
                    }

                    // Couldn't find the pending game (it should ALWAYS exist)
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
        public BoggleWordResponse PlayWord(string gameID, BoggleWord word)
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

                    if (game.GameBoard.CanBeFormed(playedWord))
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

                    // Update the state of the game
                    game.wordsPlayed.Add(playedWord);
                    playedBoggleWord.Score = wordScore;
                    user.WordsPlayed.Add(playedBoggleWord);
                    user.Score += wordScore;

                    // Compose and return the response
                    var response = new BoggleWordResponse();
                    response.Score = wordScore;

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
        /// Get game status information.
        /// 
        /// If GameID is invalid, responds with status 403 (Forbidden).
        /// 
        /// Otherwise, returns information about the game named by GameID as illustrated below.Note that the information
        /// returned depends on whether "Brief=yes" was included as a parameter as well as on the state of the game. 
        /// Responds with status code 200 (OK). Note: The Board and Words are not case sensitive.
        /// </summary>
        public BoggleGameResponse GameStatus(string gameID, string brief)
        {
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

                    if (currentGame.GameState == null || currentGame.GameState == "pending" || (currentGame.Player1 == null || currentGame.Player2 == null))
                    {
                        var response = new BoggleGameResponse();
                        response.GameState = "pending";
                        return response;
                    }
                    else if (currentGame.TimeLeft == null || currentGame.TimeLeft == 0)
                    {
                        currentGame.GameState = "completed";
                    }

                    if (brief != null && brief == "yes")
                    {
                        var briefGameStatus = new BoggleGameResponse();
                        briefGameStatus.GameState = currentGame.GameState;
                        briefGameStatus.TimeLeft = currentGame.TimeLeft;
                        briefGameStatus.Player1 = new UserResponse();
                        briefGameStatus.Player2 = new UserResponse();
                        briefGameStatus.Player1.Score = currentGame.Player1.Score;
                        briefGameStatus.Player2.Score = currentGame.Player2.Score;

                        return briefGameStatus;
                    }
                    else
                    {
                        var regGameStatus = new BoggleGameResponse();

                        regGameStatus.GameState = currentGame.GameState;
                        regGameStatus.Board = currentGame.GameBoard.ToString();
                        regGameStatus.TimeLimit = currentGame.TimeLimit;
                        regGameStatus.TimeLeft = currentGame.TimeLeft;

                        // Set player 1 properties
                        regGameStatus.Player1 = new UserResponse();
                        regGameStatus.Player1.Nickname = currentGame.Player1.Nickname;
                        regGameStatus.Player1.Score = currentGame.Player1.Score;

                        // Set player 2 properties
                        regGameStatus.Player2 = new UserResponse();
                        regGameStatus.Player2.Nickname = currentGame.Player2.Nickname;
                        regGameStatus.Player2.Score = currentGame.Player2.Score;

                        if (currentGame.GameState == "completed")
                        {
                            regGameStatus.Player1.WordsPlayed = new List<BoggleWordResponse>();

                            // Transfer the boggle words over from current game to the response
                            foreach (BoggleWord word in currentGame.Player1.WordsPlayed)
                            {
                                var formattedWord = new BoggleWordResponse();
                                formattedWord.Word = word.Word;
                                formattedWord.Score = word.Score;
                                regGameStatus.Player1.WordsPlayed.Add(formattedWord);
                            }

                            regGameStatus.Player2.WordsPlayed = new List<BoggleWordResponse>();

                            foreach (BoggleWord word in currentGame.Player2.WordsPlayed)
                            {
                                var formattedWord = new BoggleWordResponse();
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
