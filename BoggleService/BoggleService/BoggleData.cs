using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Boggle
{
    [DataContract]
    public class BoggleData
    {
        [DataMember]
        public string GameID { get; set; }

        [DataMember]
        public BoggleBoard Board { get; set; }

        [DataMember]
        public string GameState { get; set; }

        [DataMember]
        public int TimeLimit { get; set; }

        [DataMember]
        public int TimeLeft { get; set; }

        [DataMember]
        public User Player1 { get; set; }

        [DataMember]
        public User Player2 { get; set; }

        public BoggleData(BoggleBoard board, int timeLeft)
        {
            Board = board;
            GameState = "pending";
            TimeLeft = timeLeft;
        }

    }

    [DataContract]
    public class User
    {
        [DataMember]
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
        public string Word { get; set; }

        public int Score { get; set; }
    }
}