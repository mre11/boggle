// Created for CS3500, Spring 2016
// Morgan Empey (U0634576), Braden Klunker (U0725294)

using System;
using System.Text;
using System.Windows.Forms;

namespace BoggleClient
{
    /// <summary>
    /// Provides a window to start games of Boggle
    /// </summary>
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
        /// When JoiningGame is true, the only UI element which is enabled is the Cancel button.
        /// </summary>
        public bool JoiningGame
        {
            set
            {
                startOkButton.Enabled = !value;
                startCancelButton.Enabled = value;

                playerNameBox.Enabled = !value;
                serverUrlBox.Enabled = !value;
                durationUpDown.Enabled = !value;
            }
        }

        private bool validUrl = true;

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
            startCancelButton.Enabled = false;
        }

        /// <summary>
        /// Displays an error message if we could not connect to the boggle server or 
        /// the url was incorrect. 
        /// </summary>
        public void DisplayErrorMessage()
        {
            validUrl = false;
            MessageBox.Show("Error starting game.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Fired when the OK button is clicked.
        /// </summary>
        private void startOkButton_Click(object sender, EventArgs e)
        {
            if (JoinGameEvent != null)
            {
                JoinGameEvent();
                validUrl = true;
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

        /// <summary>
        /// When content is clicked in the help menu, display a help message box.
        /// </summary>
        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var builder = new StringBuilder();
            builder.AppendLine("CONNECTING");
            builder.AppendLine("1) Enter a boggle server url into the url text box.");
            builder.AppendLine("2) Enter player name into the player name box.");
            builder.AppendLine("3) Choose the duration for how long you would like to play.");
            builder.AppendLine();
            builder.AppendLine("HOW TO PLAY");
            builder.AppendLine("From Wikipedia: The game begins by shaking a covered tray of 16 cubic dice, each with a different letter printed on each of its sides. The dice settle into a 4×4 tray so that only the top letter of each cube is visible. After they have settled into the grid, a three-minute sand timer is started and all players simultaneously begin the main phase of play. Each player searches for words that can be constructed from the letters of sequentially adjacent cubes, where adjacent cubes are those horizontally, vertically, and diagonally neighboring.Words must be at least three letters long, may include singular and plural(or other derived forms) separately, but may not use the same letter cube more than once per word. Each player records all the words he or she finds by writing on a private sheet of paper. After three minutes have elapsed, all players must immediately stop writing and the game enters the scoring phase. In the scoring phase, each player reads off his or her list of discovered words.If two or more players wrote the same word, it is removed from all players' lists. Any player may challenge the validity of a word, in which case a previously nominated dictionary is used to verify or refute it. For all words remaining after duplicates have been eliminated, points are awarded based on the length of the word. The winner is the player whose point total is highest, with any ties typically broken by count of long words. One cube is printed with Qu. This is because Q is nearly always followed by U in English words (see exceptions), and if there were a Q in Boggle, it would be challenging to use if a U did not, by chance, appear next to it.For the purposes of scoring Qu counts as two letters: squid would score two points (for a five-letter word) despite being formed from a chain of only four cubes.");

            MessageBox.Show(builder.ToString(), "Boggle Help", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Handle a form closing event in the start form
        /// </summary>
        private void StartForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CancelEvent != null)
            {
                if (validUrl)
                {
                    CancelEvent();
                }
            }
        }
    }
}
