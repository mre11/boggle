using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Boggle
{
    [DataContract]
    public class BoggleGame
    {
        [DataMember(EmitDefaultValue = false)]
        public int GameID { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public BoggleBoard Board { get; set; }

        [DataMember]
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

    }

    [DataContract]
    public class User
    {
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }

        [DataMember]
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