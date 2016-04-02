// Braden Klunker, Morgan Empey, CS3500

using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace PS10
{
    /// <summary>
    /// Reprensents a BoggleGame that has a properties that describe the game, a BoggleBoard, and two Users.
    /// </summary>
    public class BoggleGame
    {
        /// <summary>
        /// Represents the GameID.
        /// </summary>
        public int GameID { get; set; }

        /// <summary>
        /// The game state can be pending, active, or completed.
        /// </summary>
        public string GameState
        {
            get
            {
                if (gameState == "active" && TimeLeft == 0)
                {
                    gameState = "completed";
                }
                return gameState;
            }
            set { gameState = value; }
        }

        /// <summary>
        /// Private memeber variable corresponsing to GameState
        /// </summary>
        private string gameState;

        /// <summary>
        /// Represents the boggle board being used in this game
        /// </summary>
        public BoggleBoard GameBoard { get; set; }

        /// <summary>
        /// The time limit in seconds for the game. Integer average of the two players timelimit requests.
        /// </summary>
        public int? TimeLimit { get; set; }

        /// <summary>
        /// The time this game started, represented as a tick count in milliseconds.
        /// </summary>
        public int TimeStarted { get; set; }

        /// <summary>
        /// The amount of time left in this game in seconds.
        /// </summary>
        public int? TimeLeft
        {
            get
            {
                var timeElapsed = (Environment.TickCount - TimeStarted) / 1000;

                if (TimeLimit < timeElapsed)
                {
                    timeLeft = 0;
                }
                else
                {
                    timeLeft = TimeLimit - timeElapsed;
                }
                return timeLeft;
            }
        }

        /// <summary>
        /// Private memeber variable corresponsing to TimeLeft
        /// </summary>
        private int? timeLeft;

        /// <summary>
        /// First user to join a pending game.
        /// </summary>
        public User Player1 { get; set; }

        /// <summary>
        /// Second user to join a pending game.
        /// </summary>
        public User Player2 { get; set; }

        /// <summary>
        /// Keeps track of the words played in this game.
        /// </summary>
        public ISet<string> wordsPlayed;

        /// <summary>
        /// Constructs a new BoggleGame from the gameID
        /// </summary>
        public BoggleGame(int gameID)
        {
            GameID = gameID;
            GameState = "pending";
            GameBoard = new BoggleBoard();
            wordsPlayed = new HashSet<string>();
        }
    }

    /// <summary>
    /// Reprensents an HTTP response for a request about a BoggleGame
    /// </summary>
    [DataContract]
    public class BoggleGameResponse
    {
        /// <summary>
        /// The ID number of this game.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int GameID { get; set; }

        /// <summary>
        /// The game state can be pending, active, or completed.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GameState { get; set; }

        /// <summary>
        /// Represents the letters from the BoggleBoard.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Board { get; set; }

        /// <summary>
        /// The time limit in seconds for the game. Integer average of the two players timelimit requests.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? TimeLimit { get; set; }

        /// <summary>
        /// The amount of time left in a game in seconds.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? TimeLeft { get; set; }

        /// <summary>
        /// First user to join a pending game.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public UserResponse Player1 { get; set; }

        /// <summary>
        /// Second user to join a pending game.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public UserResponse Player2 { get; set; }
    }

    /// <summary>
    /// Represents a user with a nickname, usertoken, total score, and the words that they have played.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Nickname of the user.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// A unique GUID for this user.
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// Total score of the user.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// List of all BoggleWords this user has played.
        /// </summary>
        public IList<BoggleWord> WordsPlayed { get; set; }

        public User()
        {
            Nickname = "";
            UserToken = "";
            Score = 0;
            WordsPlayed = new List<BoggleWord>();
        }
    }

    /// <summary>
    /// Represents an HTTP response for a request about a User.
    /// </summary>
    [DataContract]
    public class UserResponse
    {
        /// <summary>
        /// Nickname of the user.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }

        /// <summary>
        /// UserToken of the user. Allows them to communicate with the server.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }

        /// <summary>
        /// Total score of the user.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? Score { get; set; }

        /// <summary>
        /// List of all BoggleWords this user has played.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public List<BoggleWordResponse> WordsPlayed { get; set; }
    }

    /// <summary>
    /// A Boggle Word is a word that also has a score.
    /// </summary>
    public class BoggleWord
    {
        /// <summary>
        /// The user token of the user who played this BoggleWord.
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// Represents the word for this boggleword.
        /// </summary>
        public string Word { get; set; }

        /// <summary>
        /// The score for this word.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Creates an empty BoggleWord.
        /// </summary>
        public BoggleWord()
        {
            UserToken = "";
            Word = "";
            Score = 0;
        }
    }

    /// <summary>
    /// Represents an HTTP response for a BoggleWord
    /// </summary>
    [DataContract]
    public class BoggleWordResponse
    {
        /// <summary>
        /// The user token of the user who played this BoggleWord.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }

        /// <summary>
        /// Represents the word for this boggleword.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Word { get; set; }

        /// <summary>
        /// The score for this word. Score is calculated inside the boggleboard.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int Score { get; set; }
    }

    /// <summary>
    /// A JoinGameRequest is an object with all information needed to successfully join a game.
    /// </summary>
    public class JoinGameRequest
    {
        /// <summary>
        /// The user token of the requester
        /// </summary>
        public string UserToken { get; set; }

        /// <summary>
        /// The time limit that the requester has requested.
        /// </summary>
        public int TimeLimit { get; set; }
    }
}