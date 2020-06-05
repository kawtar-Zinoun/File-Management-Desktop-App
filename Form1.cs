using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManageFiles__LastOne
{
    public partial class Form1 : Form
    {
        DirectoryOptions dr1 = new DirectoryOptions();
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            dr1.showNodes();
            dr1.Callhandler();
        }
        private void NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {

            string selectedNodeText = e.Node.Text;

            if (selectedNodeText.Equals("Folders"))
            {
                listView1.Items.Clear();
                dr1.showDir();
                

            }
            else if (!e.Node.Text.Equals("Folders"))
            {
                listView1.Items.Clear();
                dr1.showFiles(selectedNodeText);
                dr1.GetId(selectedNodeText);
                dr1.ShowAlldirs(dr1.currentID);
                dr1.currentDir = selectedNodeText;
            }




        }
    }
}
