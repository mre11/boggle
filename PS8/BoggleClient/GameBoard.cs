// Created for CS3500, Spring 2016
// Morgan Empey (U0634576), Braden Klunker (U0725294)

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BoggleClient
{
    /// <summary>
    /// Provides a window to display a game of Boggle
    /// </summary>
    public partial class GameBoard : Form, IBoggleBoard
    {
        /// <summary>
        /// Represents a list of all the board spaces on the game board UI.
        /// </summary>
        private List<Label> boardSpaces;

        /// <summary>
        /// Intializes a new game board GUI.
        /// </summary>
        public GameBoard()
        {
            InitializeComponent();
            boardSpaces = new List<Label>();
            boardSpaces.Add(boardSpace0);
            boardSpaces.Add(boardSpace1);
            boardSpaces.Add(boardSpace2);
            boardSpaces.Add(boardSpace3);
            boardSpaces.Add(boardSpace4);
            boardSpaces.Add(boardSpace5);
            boardSpaces.Add(boardSpace6);
            boardSpaces.Add(boardSpace7);
            boardSpaces.Add(boardSpace8);
            boardSpaces.Add(boardSpace9);
            boardSpaces.Add(boardSpace10);
            boardSpaces.Add(boardSpace11);
            boardSpaces.Add(boardSpace12);
            boardSpaces.Add(boardSpace13);
            boardSpaces.Add(boardSpace14);
            boardSpaces.Add(boardSpace15);

        }

        /// <summary>
        /// Represents player 1's name. Associated with Player1Name text label on the GameBoard.
        /// </summary>
        public string Player1Name
        {
            get { return player1Name.Text; }
            set { player1Name.Text = value; }
        }

        /// <summary>
        /// Represents player 1's score. Associated with Player1Score text label on the GameBoard.
        /// </summary>
        public string Player1Score
        {
            get { return player1Score.Text; }
            set { player1Score.Text = value; }
        }

        /// <summary>
        /// Represents player 2's name. Associated with Player2Name text label on the GameBoard.
        /// </summary>
        public string Player2Name
        {
            get { return player2Name.Text; }
            set { player2Name.Text = value; }
        }

        /// <summary>
        /// Represents player 2's score. Associated with Player2Score text label on the GameBoard.
        /// </summary>
        public string Player2Score
        {
            get { return player2Score.Text; }
            set { player2Score.Text = value; }
        }

        /// <summary>
        /// Updates the time left label.
        /// </summary>
        public string TimeLeft
        {
            get { return timeLeft.Text; }
            set { timeLeft.Text = value; }
        }

        /// <summary>
        /// Sets the enabled state of the enter button
        /// </summary>
        public bool EnterButtonEnabled
        {
            set { gameEnterButton.Enabled = value; }
        }

        /// <summary>
        /// Sets the enabled state of the enter word text box
        /// </summary>
        public bool EnterBoxEnabled
        {
            set { wordBox.Enabled = value; }
        }

        /// <summary>
        /// Event is fired if the exit game button is pressed.
        /// </summary>
        public event Action<FormClosingEventArgs> ExitGameEvent;

        /// <summary>
        /// Event is fired when enter button is pressed.
        /// </summary>
        public event Action<string> PlayWordEvent;

        /// <summary>
        /// Closes this game window.
        /// </summary>
        public void HideWindow()
        {
            Hide();
        }

        /// <summary>
        /// Opens the GameBoard UI.
        /// </summary>
        public void ShowWindow()
        {
            Show();
        }

        /// <summary>
        /// When the enter button is clicked, the wordBox text value is sent
        /// through a delegate to the controller. The controller will then handle 
        /// the event accordingly.
        /// </summary>
        private void gameEnterButton_Click(object sender, EventArgs e)
        {
            if (PlayWordEvent != null)
            {
                PlayWordEvent(wordBox.Text);
                wordBox.Text = "";
            }
        }

        /// <summary>
        /// If the user presses the Enter key in the word box, treat it the same
        /// as pressing the "Enter" button
        /// </summary>
        private void wordBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                e.Handled = true;
                gameEnterButton_Click(sender, e);
            }            
        }

        /// <summary>
        /// When the exit button is clicked, close the window. The FormClosing event
        /// is then handled.
        /// </summary>
        private void gameExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// If the form is closed, handle it in the controller
        /// </summary>
        private void GameBoard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ExitGameEvent != null)
            {
                ExitGameEvent(e);
            }
        }

        /// <summary>
        /// Updates the boardspaces with a letter from a string of input.
        /// </summary>
        public void WriteBoardSpaces(string board)
        {
            int i = 0;
            foreach(Label space in boardSpaces)
            {
                var letter = board[i++].ToString();
                space.Text = letter == "Q" ? letter + "u" : letter;
            }            
        }
    }
}
