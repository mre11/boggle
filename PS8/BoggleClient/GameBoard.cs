﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoggleClient
{
    public partial class GameBoard : Form, IBoggleBoard
    {
        public GameBoard()
        {
            InitializeComponent();
        }

        // Possibly not needed.
        public string BoggleServerURL
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Represents player 1's name. Associated with Player1Name text label on the GameBoard.
        /// </summary>
        public string Player1Name
        {
            get
            {
                return player1Name.Text;
            }

            set
            {
                player1Name.Text = value;
            }
        }

        /// <summary>
        /// Represents player 1's score. Associated with Player1Score text label on the GameBoard.
        /// </summary>
        public string Player1Score
        {
            get
            {
                return player1Score.Text;
            }

            set
            {
                player1Score.Text = value;
            }
        }

        /// <summary>
        /// Represents player 2's name. Associated with Player2Name text label on the GameBoard.
        /// </summary>
        public string Player2Name
        {
            get
            {
                return player2Name.Text;
            }

            set
            {
                player2Name.Text = value;
            }
        }

        /// <summary>
        /// Represents player 2's score. Associated with Player2Score text label on the GameBoard.
        /// </summary>
        public string Player2Score
        {
            get
            {
                return player2Score.Text;
            }
            set
            {
                player2Score.Text = value;
            }
        }

        /// <summary>
        /// Event is fired if the exit game button is pressed.
        /// </summary>
        public event Action ExitGameEvent;

        /// <summary>
        /// Event is fired when we start a new game.
        /// </summary>
        public event Action<string, string, int> JoinGameEvent;

        /// <summary>
        /// Event is fired when enter button is pressed.
        /// </summary>
        public event Action<string> PlayWordEvent;

        /// <summary>
        /// When the enter button is clicked, the wordBox text value is sent
        /// through a delegate to the controller. The controller will then handle 
        /// the event accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gameEnterButton_Click(object sender, EventArgs e)
        {
            if(PlayWordEvent != null)
            {
                PlayWordEvent(wordBox.Text);
            }
        }

        /// <summary>
        /// When the exit button is clicked, ExitGameEvent is fired and the
        /// controller handles it accordingly.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gameExitButton_Click(object sender, EventArgs e)
        {
            if(ExitGameEvent != null)
            {
                ExitGameEvent();
            }
        }
    }
}