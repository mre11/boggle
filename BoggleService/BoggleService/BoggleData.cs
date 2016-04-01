// Braden Klunker, Morgan Empey, CS3500

using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace Boggle
{
    /// <summary>
    /// Reprensents a BoggleGame that has a properties that describe the game, a BoggleBoard, and two Users.
    /// </summary>
    public class BoggleGame
    {
        /// <summary>
        /// Represents the GameID. Initially not serialized
        /// </summary>
        public int GameID { get; set; }

        /// <summary>
        /// Shows the game state of the game.
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
        /// The time limit for the game. Integer average of the two players timelimit requests.
        /// </summary>
        public int? TimeLimit { get; set; }

        /// <summary>
        /// Once the second player joins a game, this keeps track of the time it started
        /// so we can correctly calculate the time left.
        /// </summary>
        public int TimeStarted { get; set; }

        /// <summary>
        /// The amount of time left in a game.
        /// </summary>
        public int? TimeLeft
        {
            get
            {
                var timeElapsed = (Environment.TickCount - TimeStarted) / 1000;
                var result = TimeLimit - timeElapsed;

                if (result < 0)
                {
                    timeLeft = 0;
                }
                else
                {
                    timeLeft = result;
                }
                return timeLeft;
            }
        }

        /// <summary>
        /// Private memeber variable corresponsing to TimeLeft
        /// </summary>
        private int? timeLeft;

        /// <summary>
        /// A User that is player1. First person to join a pending game.
        /// </summary>
        public User Player1 { get; set; }

        /// <summary>
        /// A User that is player2. Second person to join a pending game.
        /// </summary>
        public User Player2 { get; set; }

        /// <summary>
        /// Keeps track of the string words played in this BoggleGame.
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
        /// Represents the GameID. Initially not serialized
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int GameID { get; set; }

        /// <summary>
        /// Shows the game state of the game. Always comes first in the response.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string GameState { get; set; }

        /// <summary>
        /// Represents the letters from the BoggleBoard.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Board { get; set; }

        /// <summary>
        /// The time limit for the game. Integer average of the two players timelimit requests.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? TimeLimit { get; set; }

        /// <summary>
        /// The amount of time left in a game.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? TimeLeft { get; set; }

        /// <summary>
        /// A User that is player1. First person to join a pending game.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public UserResponse Player1 { get; set; }

        /// <summary>
        /// A User that is player2. Second person to join a pending game.
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
        /// UserToken of the user. Allows them to communicate with the server.
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
        /// The score for this word. Score is calculated inside the boggleboard.
        /// </summary>
        public int Score { get; set; }

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