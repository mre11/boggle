// Braden Klunker, Morgan Empey, CS3500

using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

// TODO add doc comments everywhere

namespace Boggle
{
    /// <summary>
    /// Reprensents a BoggleGame that has a properties that describe the game, a BoggleBoard, and two Users.
    /// </summary>
    [DataContract]
    public class BoggleGame
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
        /// String version of gamestate.
        /// </summary>
        private string gameState;

        public BoggleBoard GameBoard { get; set; }

        /// <summary>
        /// Board represents the letters from the BoggleBoard.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string Board
        {
            get
            {
                board = null;
                if (GameBoard != null)
                {
                    board = GameBoard.ToString();
                }
                return board;
            }
            set { board = value; }
        }

        private string board;

        /// <summary>
        /// The time limit for the game. Integer average of the two players timelimit requests.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public int? TimeLimit { get; set; }

        /// <summary>
        /// Once the second player joins a game, this keeps track of the time it started
        /// so we can correctly calculate the time left.
        /// </summary>
        public int TimeStarted { get; set; }

        /// <summary>
        /// The amount of time left in a game.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
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
            set { timeLeft = value; }
        }

        private int? timeLeft;

        /// <summary>
        /// A User that is player1. First person to join a pending game.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public User Player1 { get; set; }

        /// <summary>
        /// A User that is player2. Second person to join a pending game.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
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

            // Initialize the boggle board so we have the letters.
            GameBoard = new BoggleBoard();
            wordsPlayed = new HashSet<string>();
        }
        /// <summary>
        /// Empty constructor so we can decide what information we want to respond
        /// back to the client with.
        /// </summary>
        public BoggleGame() { }
    }

    /// <summary>
    /// Represents a user with a nickname, usertoken, total score, and the words that they have played.
    /// </summary>
    [DataContract]
    public class User
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
        public List<BoggleWord> WordsPlayed { get; set; }

        /// <summary>
        /// Constructs a new user with null properties.
        /// </summary>
        public User()
        {
            Nickname = null;
            UserToken = null;
            WordsPlayed = null;
        }
    }

    /// <summary>
    /// A Boggle Word is a word that also has a score.
    /// </summary>
    [DataContract]
    public class BoggleWord
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