using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BoggleClient
{
    static class Launch
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Control both game board and start form.
            GameBoard i = new GameBoard();
            StartForm j = new StartForm();
            new Controller(i, j);

            // Or new Controller(j); Use this if we change the constructor of the 
            // controller to take in 1 parameter.

            Application.Run(j);
        }
    }
}
