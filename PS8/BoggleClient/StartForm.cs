using System;
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
        public event Action<string, string, int> SendGameInfo;

        public StartForm()
        {
            InitializeComponent();
            startOkButton.Click += new EventHandler      
        }

        private void startOkButton_Click(object sender, EventArgs e)
        {
            if(SendGameInfo != null)
            {
                SendGameInfo(serverUrlBox.Text, playerNameBox.Text, (int) durationUpDown.Value);
            }
        }

        private void startCancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
