using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace Ribbon_Trail
{
    enum Language { C, CPP, CSHARP }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Prompts the user to select a directory and loads it into txtDir 
        /// </summary>
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = false; // Don't let users create new folder

            DialogResult result = fbd.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK) // Populate txtDir if successful
                txtDir.Text = fbd.SelectedPath;
        }

        /// <summary>
        /// Starts Ribbon Trail if directory and language have been provided
        /// </summary>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (txtDir.Text != "")
            {
                // Initialise and start RibbonTrail
                RibbonTrail.dir = txtDir.Text;
                RibbonTrail.Start();
            }
        }

        private void setLangC(object sender, RoutedEventArgs e)
        {
            RibbonTrail.setLanguage(Ribbon_Trail.Language.C);
        }

        private void setLangCPP(object sender, RoutedEventArgs e)
        {
            RibbonTrail.setLanguage(Ribbon_Trail.Language.CPP);
        }

        private void setLangCSharp(object sender, RoutedEventArgs e)
        {
            RibbonTrail.setLanguage(Ribbon_Trail.Language.CSHARP);
        }
    }
}
