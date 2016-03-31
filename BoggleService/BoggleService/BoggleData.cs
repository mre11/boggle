using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace Boggle
{
    [DataContract]
    public class BoggleGame
    {
        [DataMember(EmitDefaultValue = false)]
        public int GameID { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 1)]
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

        private string gameState;
        
        public BoggleBoard GameBoard { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Board
        {
            get
            {
                board = "";
                if (GameBoard != null)
                {
                    board = GameBoard.ToString();
                }
                return board;
            }
            set { board = value; }
        }

        private string board;

        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { get; set; }

        public int TimeStarted { get; set; }

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

        [DataMember(EmitDefaultValue = false)]
        public User Player1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public User Player2 { get; set; }

        /// <summary>
        /// Keeps track of the string words played in this BoggleGame.
        /// </summary>
        public ISet<string> wordsPlayed;

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

    [DataContract]
    public class User
    {
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? Score { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<BoggleWord> WordsPlayed { get; set; }

        public User()
        {
            Nickname = null;
            UserToken = null;
            //Score = 0;
            WordsPlayed = null;
        }
    }

    [DataContract]
    public class BoggleWord
    {
        /// <summary>
        /// The user token of the user who played this BoggleWord.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Word { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Score { get; set; }
    }

    public class JoinGameRequest
    {
        public string UserToken { get; set; }

        public int TimeLimit { get; set; }
    }
}