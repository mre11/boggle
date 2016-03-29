using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Boggle
{
    [DataContract]
    public class BoggleGame
    {
        [DataMember(EmitDefaultValue = false)]
        public int GameID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public BoggleBoard Board { get; set; }

        [DataMember(Order = 1)]
        public string GameState { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TimeLeft { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public User Player1 { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public User Player2 { get; set; }

        public BoggleGame(int gameID)
        {
            GameID = gameID;
            GameState = "pending";
        }


        public BoggleGame(BoggleGame newGame)
        {
            this.GameID = newGame.GameID;
            this.GameState = newGame.GameState;
            this.Board = newGame.Board;
            this.TimeLimit = newGame.TimeLimit;
            this.TimeLeft = newGame.TimeLeft;
            this.Player1 = newGame.Player1;
            this.Player2 = newGame.Player2;
        }

    }

    [DataContract]
    public class User
    {
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int Score { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public IList<BoggleWord> WordsPlayed { get; set; }
    }

    public class BoggleWord
    {
        /// <summary>
        /// The user token of the user who played this BoggleWord.
        /// </summary>
        public string UserToken { get; set; }

        public string Word { get; set; }

        public int Score { get; set; }
    }

    public class JoinGameRequest
    {
        public string UserToken { get; set; }

        public int TimeLimit { get; set; }
    }
}