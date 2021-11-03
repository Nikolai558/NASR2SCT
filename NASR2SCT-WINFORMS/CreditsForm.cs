using System;
using System.Windows.Forms;

namespace NASR_GUI
{
    public partial class CreditsForm : Form
    {
        public CreditsForm()
        {
            InitializeComponent();
        }

        private void nikolasBolingLabel_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Nikolai558");
        }

        private void kyleSandersLabel_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/KSanders7070");
        }

        private void johnLewisLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
