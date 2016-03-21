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
    public partial class StartForm : Form
    {
        /// <summary>
        /// The user-specified boggle server URL.
        /// </summary>
        public string ServerUrl
        {
            get { return serverUrlBox.Text; }
            set { serverUrlBox.Text = value; }
        }

        /// <summary>
        /// The user-specified player name.
        /// </summary>
        public string PlayerName
        {
            get { return playerNameBox.Text; }
            set { playerNameBox.Text = value; }
        }

        /// <summary>
        /// The user-specified requested game duration in seconds.
        /// </summary>
        public int RequestedDuration
        {
            get { return (int)durationUpDown.Value; }
            set { durationUpDown.Value = value; }
        }

        /// <summary>
        /// Event is fired when we start a new game.
        /// </summary>
        public event Action JoinGameEvent;

        /// <summary>
        /// Event is fired when game search is cancelled.
        /// </summary>
        public event Action CancelEvent;

        /// <summary>
        /// Initializes the StartForm component
        /// </summary>
        public StartForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fired when the OK button is clicked.
        /// </summary>
        private void startOkButton_Click(object sender, EventArgs e)
        {
            if (JoinGameEvent != null)
            {
                JoinGameEvent();
            }
        }

        /// <summary>
        /// Fired when the Cancel button is clicked.
        /// </summary>
        private void startCancelButton_Click(object sender, EventArgs e)
        {
            if (CancelEvent != null)
            {
                CancelEvent();
            }
        }
    }
}
