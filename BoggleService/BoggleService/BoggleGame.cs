using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Boggle
{
    public class BoggleGame
    {
        public BoggleBoard Board { get; set; }

        public string GameState { get; set; }

        public int TimeLeft { get; set; }

        public Dictionary<string, int> player1, player2;

        public BoggleGame(BoggleBoard board, int timeLeft)
        {
            Board = board;
            GameState = "pending";
            TimeLeft = timeLeft;
        }

    }

    public class Player
    {
        public string nickname { get; set; }

        public int score { get; set; }

        //public Dictionary<string, int> wordsPlayed = new Dictionary<string, int>();
    }
}