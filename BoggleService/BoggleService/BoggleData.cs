using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Boggle
{
    [DataContract]
    public class BoggleGame
    {
        [DataMember(EmitDefaultValue = false)]
        public int GameID { get; set; }

        [DataMember(Order = 1)]
        public string GameState { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public BoggleBoard Board { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { get; set; }

        public System.DateTime time { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int TimeLeft { get { return TimeLeft; } set { TimeLeft = (int)System.DateTime.Now.Subtract(time).TotalSeconds; } }

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