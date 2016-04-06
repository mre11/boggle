// Braden Klunker, Morgan Empey, CS3500

using System;
using System.Data.SqlClient;
using System.IO;
using System.ServiceModel.Web;
using System.Net;
using System.Configuration;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    /// <summary>
    /// Implements a RESTful service for a Boggle game
    /// </summary>
    public class BoggleService : IBoggleService
    {
        /// <summary>
        /// Database connection string
        /// </summary>
        private static string BoggleDB;

        static BoggleService()
        {
            BoggleDB = ConfigurationManager.ConnectionStrings["BoggleDB"].ConnectionString;
        }

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
            InitializePendingGame();

            // Error check users information before setting up SQL connection.
            if (requestedUser.Nickname == null || requestedUser.Nickname.Trim() == "")
            {
                SetStatus(Forbidden);
                return null;
            }

            // Open SQL connection to BoggleDB
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();

                // Open SQL transaction and begin transaction
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    // Open SQL command with SQL code
                    var cmd = "INSERT INTO Users(UserToken, Nickname) VALUES(@UserToken, @Nickname)";
                    using (SqlCommand command = new SqlCommand(cmd, conn, trans))
                    {
                        string userToken = Guid.NewGuid().ToString();
                        // Set command parameters with AddWithValue
                        command.Parameters.AddWithValue("@UserToken", userToken);
                        command.Parameters.AddWithValue("@Nickname", requestedUser.Nickname.Trim());

                        // Execute a non query and if successful set status code
                        // We may not need to execute non query because we already know the nickname is 
                        // correct and that the userToken will be correct.
                        command.ExecuteNonQuery();
                        SetStatus(Created);

                        // Create formatted response to send back
                        var response = new UserResponse();
                        response.UserToken = userToken;

                        // Commit users information into the database
                        trans.Commit();
                        return response;
                    }
                }
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
            InitializePendingGame();

            // Make sure game is >= 5 and <= 120 and the user token is valid
            if (requestBody.UserToken == null || requestBody.UserToken == ""
                || requestBody.TimeLimit < 5 || requestBody.TimeLimit > 120)
            {
                SetStatus(Forbidden);
                return null;
            }

            var pendingGameID = -1;
            bool createNewPendingGame = false;

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    // Check that the user has been created
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Users WHERE UserToken = @Player", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player", requestBody.UserToken);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                SetStatus(Forbidden);
                                return null;
                            }
                        }
                    }

                    var readPending = "SELECT GameID, Player1, Player2, TimeLimit FROM Game WHERE GameState = 'pending'";
                    var updatePending = "UPDATE Game SET ";
                    bool activeGame = false;

                    using (SqlCommand command = new SqlCommand(readPending, conn, trans))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pendingGameID = (int)reader["GameID"];

                                if (reader["Player1"] == DBNull.Value)
                                {

                                    updatePending += "Player1 = @Player, TimeLimit = @TimeLimit WHERE Game.GameState = 'pending'";
                                    SetStatus(Accepted);
                                }
                                else if ((string)reader["Player1"] == requestBody.UserToken)
                                {
                                    SetStatus(Conflict);
                                    return null;
                                }
                                else if (reader["Player2"] == DBNull.Value)
                                {
                                    activeGame = createNewPendingGame = true;

                                    // Calculate the average timelimit between the two players.
                                    int averageTimeLimit = ((int)reader["TimeLimit"] + requestBody.TimeLimit) / 2;

                                    updatePending += "Player2 = @Player, TimeLimit = " + averageTimeLimit + ", StartTime = @StartTime, GameState = 'active' WHERE Game.GameState = 'pending'";
                                    SetStatus(Created);
                                }
                            }
                        }
                    }

                    using (SqlCommand command = new SqlCommand(updatePending, conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player", requestBody.UserToken);

                        // If a game only has 1 player, it will not be an active game.
                        // Otherwise the game now has 2 players and needs TimeLimit = (TimeLimit1 + TimeLimit2)/2,
                        // TimeStarted = Current Time, and GameState = 'active'.
                        if (!activeGame)
                        {
                            // Set command parameters and execute query
                            command.Parameters.AddWithValue("@TimeLimit", requestBody.TimeLimit);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@StartTime", Environment.TickCount);
                        }
                        command.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
            }

            if (createNewPendingGame)
            {
                InitializePendingGame();
            }

            // Return the response
            var response = new BoggleGameResponse();
            response.GameID = pendingGameID;
            return response;
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
            InitializePendingGame();

            // Validate user token
            if (user.UserToken == null || user.UserToken == "")
            {
                SetStatus(Forbidden);
                return;
            }

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    // Get Player1 user token out of the pending game
                    using(SqlCommand command = new SqlCommand("SELECT Player1 FROM Game WHERE GameState = 'pending'", conn, trans))
                    {
                        using(SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // User token is not a player in the pending game
                                if (reader["Player1"] == DBNull.Value || (string)reader["Player1"] != user.UserToken)
                                {
                                    SetStatus(Forbidden);
                                    return;
                                }
                            }
                        }
                    }

                    // Set Player1 in pending game to null
                    using (SqlCommand command = new SqlCommand("UPDATE Game SET Player1 = @Player WHERE Game.GameState = 'pending'", conn, trans))
                    {
                        command.Parameters.AddWithValue("@Player", DBNull.Value);
                        command.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
            }

            SetStatus(OK);
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
            InitializePendingGame();

            // TODO implement PlayWord

            // Word or UserToken is invalid
            int intGameID;
            if (word.Word == null || word.UserToken == null|| word.Word.Trim() == ""
                || word.UserToken == "" || !int.TryParse(gameID, out intGameID))
            {
                SetStatus(Forbidden);
                return null;
            }

            var board = "";

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    var cmd = "SELECT * FROM Users WHERE UserToken = @UserToken";
                    using (SqlCommand command = new SqlCommand(cmd, conn, trans))
                    {                        
                        command.Parameters.AddWithValue("@UserToken", word.UserToken);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows) // User does not exist
                            {
                                SetStatus(Forbidden);
                                return null;
                            }
                        }                          
                        
                    }

                    cmd = "SELECT * FROM Game WHERE GameID = @GameID";
                    using (SqlCommand command = new SqlCommand(cmd, conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", gameID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (word.UserToken != (string)reader["Player1"] &&
                                        word.UserToken != (string)reader["Player2"])
                                    {
                                        // User token isn't a player in the game
                                        SetStatus(Forbidden);
                                        return null;
                                    }

                                    if ((string)reader["GameState"] != "active"
                                        || GameIsCompleted((int)reader["StartTime"], (int)reader["TimeLimit"]))
                                    {
                                        // Game state isn't active
                                        SetStatus(Conflict);
                                        return null;
                                    }

                                    // Save off the value of the game board to use for scoring
                                    board = (string)reader["Board"];
                                }
                            }
                            else // Game does not exist
                            {
                                SetStatus(Forbidden);
                                return null;
                            }
                        }
                    }

                    trans.Commit();
                }
            }

            int wordScore = 0;
            var gameBoard = new BoggleBoard(board);

            var playedWord = word.Word;
            playedWord = playedWord.Trim();
            playedWord = playedWord.ToLower();

            if (gameBoard.CanBeFormed(playedWord))
            {
                if (WordHasBeenPlayed(intGameID, playedWord))
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

            AddWordRecord(playedWord, intGameID, word.UserToken, wordScore);

            // Compose and return the response            
            var response = new BoggleWordResponse();
            response.Score = wordScore;

            return response;
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
            InitializePendingGame();
            throw new NotImplementedException();

            // TODO implement GameStatus
            // TODO I removed Score from the Users table.  We should just compute it on the fly here from the Words table.

            // Error check gameID and Brief information before setting up SQL connection.

            // Open SQL connection to BoggleDB
            // Open SQL transaction and begin transaction
            // Open SQL command with SQL code

            // Set command parameters with AddWithValue

            // Use SqlDataReader reader = command.ExecuteReader to get information of of database.

            // Execute a non query and if successful set status code

            // Commit the transaction and return the UserResponse.


            //try
            //{
            //    lock (sync)
            //    {
            //        InitializePendingGame();

            //        // Invalid gameID or the game doesn't exist
            //        int intGameID;
            //        BoggleGame currentGame;

            //        if (!int.TryParse(gameID, out intGameID) || !games.TryGetValue(intGameID, out currentGame))
            //        {
            //            SetStatus(Forbidden);
            //            return null;
            //        }

            //        SetStatus(OK);

            //        if (currentGame.GameState == null || currentGame.GameState == "pending" || (currentGame.Player1 == null || currentGame.Player2 == null))
            //        {
            //            var response = new BoggleGameResponse();
            //            response.GameState = "pending";
            //            return response;
            //        }
            //        else if (currentGame.TimeLeft == null || currentGame.TimeLeft == 0)
            //        {
            //            currentGame.GameState = "completed";
            //        }

            //        if (brief != null && brief == "yes")
            //        {
            //            var briefGameStatus = new BoggleGameResponse();
            //            briefGameStatus.GameState = currentGame.GameState;
            //            briefGameStatus.TimeLeft = currentGame.TimeLeft;
            //            briefGameStatus.Player1 = new UserResponse();
            //            briefGameStatus.Player2 = new UserResponse();
            //            briefGameStatus.Player1.Score = currentGame.Player1.Score;
            //            briefGameStatus.Player2.Score = currentGame.Player2.Score;

            //            return briefGameStatus;
            //        }
            //        else
            //        {
            //            var regGameStatus = new BoggleGameResponse();

            //            regGameStatus.GameState = currentGame.GameState;
            //            regGameStatus.Board = currentGame.GameBoard.ToString();
            //            regGameStatus.TimeLimit = currentGame.TimeLimit;
            //            regGameStatus.TimeLeft = currentGame.TimeLeft;

            //            // Set player 1 properties
            //            regGameStatus.Player1 = new UserResponse();
            //            regGameStatus.Player1.Nickname = currentGame.Player1.Nickname;
            //            regGameStatus.Player1.Score = currentGame.Player1.Score;

            //            // Set player 2 properties
            //            regGameStatus.Player2 = new UserResponse();
            //            regGameStatus.Player2.Nickname = currentGame.Player2.Nickname;
            //            regGameStatus.Player2.Score = currentGame.Player2.Score;

            //            if (currentGame.GameState == "completed")
            //            {
            //                regGameStatus.Player1.WordsPlayed = new List<BoggleWordResponse>();

            //                // Transfer the boggle words over from current game to the response
            //                foreach (BoggleWord word in currentGame.Player1.WordsPlayed)
            //                {
            //                    var formattedWord = new BoggleWordResponse();
            //                    formattedWord.Word = word.Word;
            //                    formattedWord.Score = word.Score;
            //                    regGameStatus.Player1.WordsPlayed.Add(formattedWord);
            //                }

            //                regGameStatus.Player2.WordsPlayed = new List<BoggleWordResponse>();

            //                foreach (BoggleWord word in currentGame.Player2.WordsPlayed)
            //                {
            //                    var formattedWord = new BoggleWordResponse();
            //                    formattedWord.Word = word.Word;
            //                    formattedWord.Score = word.Score;
            //                    regGameStatus.Player2.WordsPlayed.Add(formattedWord);
            //                }
            //            }
            //            return regGameStatus;
            //        }
            //    }
            //}
            //catch (Exception)
            //{
            //    SetStatus(InternalServerError);
            //    return null;
            //}
        }

        /// <summary>
        /// If the games record is empty, add a game as the first pending game.
        /// </summary>
        private void InitializePendingGame()
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                bool createPendingGame = false;

                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand cmd = new SqlCommand("SELECT GameState FROM Game WHERE GameState = 'pending'", conn, trans))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                createPendingGame = true;
                            }
                        }
                    }

                    if (createPendingGame)
                    {
                        using (SqlCommand cmd = new SqlCommand("INSERT INTO Game(Board) VALUES (@Board)", conn, trans))
                        {
                            BoggleBoard temp = new BoggleBoard();
                            cmd.Parameters.AddWithValue("@Board", temp.ToString());
                            cmd.ExecuteNonQuery();
                        }

                        // Only need to commit if we made a new pending game. Otherwise 
                        // just let it flow through.
                        trans.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if more time has elapsed than the time limit since the start time.
        /// The start time is expected in milliseconds, while the time limit is expected in seconds.
        /// </summary>
        private bool GameIsCompleted(int startTime, int timeLimit)
        {
            return (Environment.TickCount - startTime) / 1000 - timeLimit > 0;
        }

        /// <summary>
        /// Returns true if the specified word has already been played in the game specified by GameID.
        /// </summary>
        private bool WordHasBeenPlayed(int intGameID, string word)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    var cmd = "SELECT * FROM Words WHERE GameID = @GameID AND Word = @Word";
                    using (SqlCommand command = new SqlCommand(cmd, conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", intGameID);
                        command.Parameters.AddWithValue("@Word", word);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if(reader.HasRows)
                            {
                                return true;
                            }
                        }
                        trans.Commit();
                    }
                }

            }
            return false;
        }

        /// <summary>
        /// Adds a row to the Words table with the given data
        /// </summary>
        private void AddWordRecord(string word, int gameID, string userToken, int score)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                
                var cmd = "INSERT INTO Words(Word, GameID, Player, Score) VALUES(@Word, @GameID, @Player, @Score)";
                using (SqlCommand command = new SqlCommand(cmd, conn))
                {
                    command.Parameters.AddWithValue("@Word", word);
                    command.Parameters.AddWithValue("@GameID", gameID);
                    command.Parameters.AddWithValue("@Player", userToken);
                    command.Parameters.AddWithValue("@Score", score);

                    command.ExecuteNonQuery();
                }                
            }
        }
    }
}
