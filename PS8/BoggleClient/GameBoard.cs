﻿using System;
using System.Windows.Forms;

namespace BoggleClient
{
    public partial class GameBoard : Form, IBoggleBoard
    {
        private StartForm newGame;

        public GameBoard()
        {
            InitializeComponent();
            
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

        // TODO need access to the 16 boggle game labels

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
        public void OpenWindow()
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
            }
        }

        /// <summary>
        /// When the exit button is clicked, close the window. The FormClosing event
        /// is then handled.
        /// </summary>
        private void gameExitButton_Click(object sender, EventArgs e)
        {
            Close(); // handle the FormClosing event
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
    }
}
