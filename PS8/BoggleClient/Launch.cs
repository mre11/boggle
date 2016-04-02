// Created for CS3500, Spring 2016
// Morgan Empey (U0634576), Braden Klunker (U0725294)

using System;
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
            GameBoard gameBoard = new GameBoard();
            StartForm startWindow = new StartForm();
            new Controller(gameBoard, startWindow);

            // Or new Controller(j); Use this if we change the constructor of the 
            // controller to take in 1 parameter.

            Application.Run(startWindow);
        }
    }
}
