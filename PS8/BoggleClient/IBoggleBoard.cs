using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoggleClient
{
    interface IBoggleBoard
    {
        /// <summary>
        /// Event is fired when enter button is pressed.
        /// </summary>
        event Action<string> PlayWordEvent;

        /// <summary>
        /// Event is fired if the exit game button is pressed.
        /// </summary>
        event Action ExitGameEvent;

        /// <summary>
        /// Event is fired when we start a new game.
        /// </summary>
        event Action<string, string, int> JoinGameEvent;

        string BoggleServerURL { get; set;}

        /// <summary>
        /// Represents player 1's name. Associated with Player1Name text label on the GameBoard.
        /// </summary>
        string Player1Name { get; set; }

        /// <summary>
        /// Represents player 1's score. Associated with Player1Score text label on the GameBoard.
        /// </summary>
        string Player1Score { get; set; }

        /// <summary>
        /// Represents player 2's name. Associated with Player2Name text label on the GameBoard.
        /// </summary>
        string Player2Name { get; set; }

        /// <summary>
        /// Represents player 2's score. Associated with Player2Score text label on the GameBoard.
        /// </summary>
        string Player2Score { get; set; }       

    } 
}
