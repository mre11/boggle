using System;
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
        /// Property that enables the controller to make the all text boxes on the Start Form
        /// read only.
        /// </summary>
        public bool DisableStartTextBoxes
        {
            set
            {
                if(value)
                {
                    playerNameBox.ReadOnly = true;
                    serverUrlBox.ReadOnly = true;
                    durationUpDown.ReadOnly = true;
                    durationUpDown.Enabled = false;
                }
                else
                {
                    playerNameBox.ReadOnly = false;
                    serverUrlBox.ReadOnly = false;
                    durationUpDown.ReadOnly = false;
                    durationUpDown.Enabled = true;
                }

            }
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
            startCancelButton.Enabled = false;
        }

        /// <summary>
        /// Displays an error message if we could not connect to the boggle server or 
        /// the url was incorrect. 
        /// </summary>
        public void DisplayErrorMessage()
        {
            MessageBox.Show("Invalid Boggle server url.", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

            // Re-enable input components
            startOkButton.Enabled = true;
            startCancelButton.Enabled = false;
            DisableStartTextBoxes = false;
        }

        /// <summary>
        /// Fired when the OK button is clicked.
        /// </summary>
        private void startOkButton_Click(object sender, EventArgs e)
        {
            if (JoinGameEvent != null)
            {
                startOkButton.Enabled = false;
                startCancelButton.Enabled = true;
                DisableStartTextBoxes = true;
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
                startCancelButton.Enabled = false;
                startOkButton.Enabled = true;
                DisableStartTextBoxes = false;
                CancelEvent();
            }
        }

        /// <summary>
        /// When content is clicked in the help menu, display a help message box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // TODO: Modify contents of help message box or create a new form with drop down items for help information.
            MessageBox.Show("Welcome", "Help", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Handle a form closing event in the start form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // TODO: Make sure that if we connect then click exit, we don't get an exception from inside of the controller.
        }
    }
}
