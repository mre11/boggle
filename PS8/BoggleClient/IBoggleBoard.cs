// Created for CS3500, Spring 2016
// Morgan Empey (U0634576), Braden Klunker (U0725294)

using System;
using System.Windows.Forms;

namespace BoggleClient
{
    /// <summary>
    /// Provides an interface for a Boggle game board window
    /// </summary>
    public interface IBoggleBoard
    {
        /// <summary>
        /// Event is fired when enter button is pressed.
        /// </summary>
        event Action<string> PlayWordEvent;

        /// <summary>
        /// Event is fired if the exit game button is pressed.
        /// </summary>
        event Action<FormClosingEventArgs> ExitGameEvent;

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

        /// <summary>
        /// Updates the time left label.
        /// </summary>
        string TimeLeft { get; set; }

        /// <summary>
        /// Sets the enabled state of the enter button
        /// </summary>
        bool EnterButtonEnabled { set; }

        /// <summary>
        /// Sets the enabled state of the enter word text box
        /// </summary>
        bool EnterBoxEnabled { set; }

        /// <summary>
        /// Closes this IBoggleBoard window.
        /// </summary>
        void HideWindow();

        /// <summary>
        /// Opens the GameBoard UI.
        /// </summary>
        void ShowWindow();

        /// <summary>
        /// Updates the boardspaces with a letter from a string of input.
        /// </summary>
        void WriteBoardSpaces(string board);
    } 
}
