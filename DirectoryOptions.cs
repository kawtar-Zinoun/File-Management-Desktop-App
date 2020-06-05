using System;
using System.Data;
using MySql.Data.MySqlClient;
using System.Windows.Forms;


namespace ManageFiles__LastOne
{
    class DirectoryOptions 
    {
        string afterN; // after renaming
        string beforeN; // before renaming
        Connexion cr = new Connexion(); 
        DataTable dtTree = new DataTable(); 
        int id; //for the nodes
        int idD; // 
        public int currentID = 0; // ID current clicked directory
        string dirName; // directory name
       public string currentDir; // current clicked directory
        FileOptions fl = new FileOptions();
        ListViewItem cutItem = new ListViewItem();

        public void showNodes() // to show nodes in treeview
        {
            cr.ToConnect();
            string stg = "select Dir_Name, ID_Dir from directories where Id_Parent is null";
            MySqlCommand cmd = new MySqlCommand(stg, cr.conn);
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            frm1.treeView1.BeginUpdate();
            frm1.treeView1.ContextMenuStrip = frm1.contextMenuStrip1;
            try
            {
                cmd.CommandType = CommandType.Text;
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dtTree);
                da.Dispose();
                foreach (DataRow dr in dtTree.Rows)
                {

                    dirName = dr["Dir_Name"].ToString();
                    TreeNode tn = new TreeNode(dirName);
                    tn.Name = dirName; // treeName is directory name
                    id = Convert.ToInt32(dr["ID_Dir"]);
                    frm1.treeView1.Nodes["Folders"].Nodes.Add(tn);

                    AddNodes(id, tn);



                }
                frm1.treeView1.EndUpdate();

               
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public void AddNodes(int dirId, TreeNode actualNode) // add nodes inside nodes
            // call in shownodes()
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            
            cr.ToConnect();
         
            DataTable dt3 = new DataTable();
           
            string stg3 = "select Dir_Name, ID_Dir from directories where Id_Parent = @dr";
            MySqlCommand cmd3 = new MySqlCommand(stg3, cr.conn);
            cmd3.Parameters.AddWithValue("@dr", dirId);
            cmd3.CommandType = CommandType.Text;
            MySqlDataAdapter dar2 = new MySqlDataAdapter(cmd3);
            dar2.Fill(dt3);
            dar2.Dispose();
            foreach (DataRow dr3 in dt3.Rows)
            {
                string dirN = dr3["Dir_Name"].ToString();
                int idd = Convert.ToInt32(dr3["ID_Dir"]);
                TreeNode tnode = new TreeNode(dirN);
                tnode.Name = dirN;
                tnode.ImageIndex = 1;
                // frm1.treeView1.Nodes["Folders"].Nodes[actualNode.Index].Nodes.Add(tnode);
                actualNode.Nodes.Add(tnode);

             AddNodes(idd, tnode); //recursive function to addnodes again
            }
              DataTable dt2 = new DataTable();
           string stg2 = "select File_Name from files, directories where ID_Dir = @dr and Parent_id = ID_Dir";

           MySqlCommand cmd2 = new MySqlCommand(stg2, cr.conn);
           cmd2.Parameters.AddWithValue("@dr", dirId);
           cmd2.CommandType = CommandType.Text;
           MySqlDataAdapter dar = new MySqlDataAdapter(cmd2);
           dar.Fill(dt2);
           dar.Dispose();
           foreach (DataRow dr2 in dt2.Rows)
           {
               string fileName = dr2["File_Name"].ToString();

               TreeNode tnode = new TreeNode(fileName);
               tnode.ImageIndex = 0;
              
               actualNode.Nodes.Add(tnode);

           } 

        }
        public void showDir() //show directories in listview
        {

            cr.ToConnect();
            string stg = "select Dir_Name from directories where Id_Parent is null";
            MySqlCommand cmd = new MySqlCommand(stg, cr.conn);

            try
            {
                Form1 frm1 = (Form1)Application.OpenForms["Form1"];

              MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dp = new DataTable();
                da.Fill(dp);
                
                foreach (DataRow dr in dp.Rows)
                {
                    frm1.listView1.View = View.Details;
                    ListViewItem Dir = new ListViewItem(dr["Dir_Name"].ToString());
                    frm1.listView1.View = View.LargeIcon;
            
                    Dir.ImageIndex = 3;
                   
                    frm1.listView1.Items.Add(Dir);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
           
        }
        public void Callhandler() //functions for all handlers to avoid firing event two times
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            frm1.listView1.DoubleClick += Tb_clicked; // doubleclick on item
          //frm1.listView1.ContextMenuStrip = frm1.contextMenuStrip1;
            frm1.listView1.MouseUp += ItemClick; // rightclick
            frm1.addNewDirectoryToolStripMenuItem.Click += Tool_Clicked; // menu strip add dir
            frm1.Rename.Click += RenameDir; // rename in the context menu strip
            frm1.Delete.Click += DeleteFolder; // delete in the context menu strip
            frm1.copyToolStripMenuItem.Click += CutItem;
            frm1.pasteToolStripMenuItem1.Click += pasteItem;
            frm1.listView1.AfterLabelEdit += after_label; // after renaming

        }
        public void CutItem()
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            string selec = frm1.listView1.SelectedItems[0].Text;
            if (frm1.listView1.SelectedItems.Count == 0)
                return;

            else if (frm1.listView1.SelectedItems[0].ImageIndex == 3) // imageIndex = 3 then we cut dir
            {
                cutItem = frm1.listView1.SelectedItems[0];
                frm1.listView1.SelectedItems[0].Remove();
                frm1.listView1.Refresh();
                foreach (TreeNode node in frm1.treeView1.Nodes)
                {
                    if (node.Text == selec)
                    {
                        frm1.treeView1.SelectedNode = node;
                        frm1.treeView1.Nodes.Remove(node);
                        frm1.treeView1.Refresh();
                    }
                    else
                    {
                        DeleteFromTV(selec, node);

                    }
                }
                DeleteDir(selec);
                
            }
        }
        public void PastemyItem(ListViewItem it)
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];

            frm1.listView1.Items.Add(it);
            GetId(it.Text);
            AddDirToBd(it.Text, currentID);

            
            if (String.IsNullOrEmpty(currentDir))
            {
                frm1.treeView1.Nodes["Folders"].Nodes.Add(it.Text);
            }
            foreach (TreeNode n in frm1.treeView1.Nodes)
            {
                if (n.Text == currentDir)
                {
                    n.Nodes.Add(it.Text);
                    frm1.treeView1.Refresh();
                }
                else addDirtoTree(currentDir, n, it.Text); // search in node and add
            }
            frm1.listView1.Refresh();
        }

        private void CutItem(object sender, EventArgs e)
        {   
            CutItem();
        }
        private void pasteItem(object sender, EventArgs e)
        {
            PastemyItem(cutItem);
        }
        
        private void ItemClick(object sender, MouseEventArgs e) // for contextmenustrip -- blank or item
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            if (e.Button == MouseButtons.Right)
            { if (frm1.listView1.SelectedItems.Count > 0)
                {
                    ListViewItem item = frm1.listView1.GetItemAt(e.X, e.Y);
                    frm1.contextMenuStrip2.Show(frm1.listView1, e.Location);

                }
                else { frm1.contextMenuStrip1.Show(frm1.listView1, e.Location); }
            }
            
            
        }
        private void RenameDir(object sender, EventArgs e) // rename  in contextmenustrip
        {
           // ToolStripItem menuItem = sender as ToolStripItem;
           
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            if (frm1.listView1.SelectedItems.Count>0) {
                ListViewItem item = frm1.listView1.SelectedItems[0];
                beforeN = item.Text;
                item.BeginEdit();


            }
        }
        // ------------------------ For Renaming ----------------------------------------- 
        private void after_label(object sender, LabelEditEventArgs e) // after renaming
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            if (string.IsNullOrWhiteSpace(e.Label))
            {
                e.CancelEdit = true;
                MessageBox.Show("Please enter a valid value.");
                return;
            }
            else if (frm1.listView1.FindItemWithText(e.Label) != null) 
            {
                e.CancelEdit = true;
                MessageBox.Show("Name already exists.");

            }
            else
            {

                beforeN = frm1.listView1.SelectedItems[0].Text;
                afterN = e.Label;
                // TreeNode[] tns = frm1.treeView1.Nodes.Find(beforeN, true);

                foreach (TreeNode n in frm1.treeView1.Nodes)
                {
                    if (n.Text == beforeN)
                    {
                        frm1.treeView1.SelectedNode = n;

                        n.Text = afterN;
                        n.Name = afterN;
                        frm1.treeView1.Refresh();
                    }
                    else findAndRenameNodes(n, beforeN);

                }
                /*  if (tns.Length > 0)
                  {

                      tns[0].Text = afterN;
                      tns[0].Name = afterN;
                  frm1.treeView1.Refresh();

                  } */

                Rename(beforeN, afterN);

            }
        
        }
        public void findAndRenameNodes(TreeNode treeNode, String FindText) // find and rename inside node
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            foreach (TreeNode n in treeNode.Nodes)
            {
                if  (n.Text == FindText)
                {
                    frm1.treeView1.SelectedNode = n;
                    n.Text = afterN;
                    n.Name = afterN; }
                else findAndRenameNodes(n, FindText);
            }
            }
        public void Rename(string oldName, string newName) // renaming in BD
        {
            string query = "Update directories set Dir_Name = @rn where Dir_Name = @rp";
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@rn", newName);
            cmd.Parameters.AddWithValue("@rp", oldName);
            cmd.Connection = cr.conn;
            cmd.ExecuteNonQuery();
        }
      
        public void GetId(string name) // getID of a certain dir name
        {
            
            cr.ToConnect();
            string query = "select ID_Dir from directories where Dir_Name = @rp";
            MySqlCommand cmd = new MySqlCommand(query, cr.conn);
            
            cmd.Parameters.AddWithValue("@rp", name);
            
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                DataTable dp = new DataTable();
                da.Fill(dp);
                da.Dispose();
                foreach (DataRow dr in dp.Rows)
                {
                    currentID = Convert.ToInt32(dr["ID_Dir"]);
                }
               
            
            
        }
        private void Tool_Clicked(object sender, EventArgs e) // add dir context menu strip
        { addDirManually(); }
        private void Tb_clicked(object sender, EventArgs e) //double click on item = show its elements
        {
            ListView ls = sender as ListView;
            if (ls.SelectedItems.Count == 0)
                return;

            else if ((ls.SelectedItems.Count > 0) && (ls.SelectedItems[0].ImageIndex == 3))
                dirName = ls.SelectedItems[0].Text;
            currentDir = dirName;
            GetId(currentDir);
            showFiles(dirName);
            ShowAlldirs(currentID);

        }
        public void addDirManually() // add dir in listview + bd + treeview
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];

            ListViewItem dirr = new ListViewItem();
            dirr.Text = "new folder";
            frm1.listView1.View = View.LargeIcon;

            dirr.ImageIndex = 3;
            AddDirToBd(dirr.Text,currentID);

            frm1.listView1.Items.Add(dirr);
            if (String.IsNullOrEmpty(currentDir))
            {
                frm1.treeView1.Nodes["Folders"].Nodes.Add(dirr.Text);
            }
            foreach (TreeNode n in frm1.treeView1.Nodes)
            {   
               if (n.Text == currentDir)
                {
                    n.Nodes.Add(dirr.Text);
                    frm1.treeView1.Refresh();
                }
                else addDirtoTree(currentDir, n, dirr.Text); // search in node and add
            }
            
            frm1.listView1.Refresh();

        }
        public void addDirtoTree(string parentN, TreeNode treenode, string nameNode) // search in node and add
        { 
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];

            foreach (TreeNode no in treenode.Nodes)
            {
                if (no.Text == parentN)
                {
                    no.Nodes.Add(nameNode);
                    frm1.treeView1.Refresh();

                }
                else addDirtoTree(parentN, no, nameNode);
            }
        }
    
        public void AddDirToBd(string dirName, int parent) // add dir to bd
        {   if (parent != 0) { 
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = cr.conn;

            cmd.CommandText = "INSERT INTO directories(Dir_Name,Id_Parent) VALUES(@Name, @foreignID)"; 
            cmd.Prepare();
            cmd.Parameters.AddWithValue("@Name", dirName);
            cmd.Parameters.AddWithValue("@foreignID", parent);
           
            cmd.ExecuteNonQuery();
            }
        else {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = cr.conn;

                cmd.CommandText = "INSERT INTO directories(Dir_Name) VALUES(@Name)";
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@Name", dirName);
                

                cmd.ExecuteNonQuery();

            }
        }
        public void showFiles(string nomDir)
        {
            
            cr.ToConnect();
            string stg = "select File_Name, ID_Dir from files, directories where Dir_Name = @dr and Parent_id = ID_Dir";
            MySqlCommand cmd = new MySqlCommand(stg, cr.conn);
            cmd.Parameters.AddWithValue("@dr", nomDir);
            
            try
            {
                Form1 frm1 = (Form1)Application.OpenForms["Form1"];
                frm1.listView1.Items.Clear();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                System.Data.DataTable dt = new System.Data.DataTable();
                da.Fill(dt);
               
                foreach (DataRow dr in dt.Rows)
                {
                    frm1.listView1.View = View.Details;
                    string filName =  dr["File_Name"].ToString();
                   idD = Convert.ToInt32(dr["ID_Dir"]);
                    ListViewItem file = new ListViewItem(filName);
                    
                    frm1.listView1.View = View.LargeIcon;
                    
                    file.ImageIndex = 1;

                    frm1.listView1.Items.Add(file);
                 
                }
                
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            }
        public void ShowAlldirs(int directoryID)
        {
            cr.ToConnect();
            string stgx = "select Dir_Name from directories where Id_Parent = @dr";
            MySqlCommand cmdx = new MySqlCommand(stgx, cr.conn);
            cmdx.Parameters.AddWithValue("@dr", directoryID);

            try
            {
                Form1 frm1 = (Form1)Application.OpenForms["Form1"];
                
                MySqlDataAdapter da = new MySqlDataAdapter(cmdx);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    frm1.listView1.View = View.Details;
                    string drName = dr["Dir_Name"].ToString();

                    ListViewItem drn = new ListViewItem(drName);

                    frm1.listView1.View = View.LargeIcon;

                    drn.ImageIndex = 3;

                    frm1.listView1.Items.Add(drn);

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

        } // --------------------------- Delete functions and events ----------------------------------
        private void DeleteFolder(object sender, EventArgs e) // delete folder from listview and treeview and bd
        {
            ListView lso = sender as ListView;
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];
            string selec = frm1.listView1.SelectedItems[0].Text;
            if (frm1.listView1.SelectedItems.Count == 0)
                return;

            else if (frm1.listView1.SelectedItems[0].ImageIndex == 3) // imageIndex = 3 then we remove dir
            {
               
                frm1.listView1.SelectedItems[0].Remove();
                frm1.listView1.Refresh();

                foreach (TreeNode node in frm1.treeView1.Nodes)
                {
                    if (node.Text == selec)
                    {
                        frm1.treeView1.SelectedNode = node;
                        frm1.treeView1.Nodes.Remove(node);
                        frm1.treeView1.Refresh();
                    }
                    else
                    {
                        DeleteFromTV(selec, node);

                    }
                }
               
                DeleteDir(selec);
            }
            else if ((frm1.listView1.SelectedItems.Count > 0) && (frm1.listView1.SelectedItems[0].ImageIndex == 1)) // 1 then remove file
            {
                frm1.listView1.SelectedItems[0].Remove();
                frm1.listView1.Refresh();
                foreach (TreeNode node in frm1.treeView1.Nodes)
                {
                    if (node.Text == selec)
                    {
                        frm1.treeView1.SelectedNode = node;
                        frm1.treeView1.Nodes.Remove(node);
                        frm1.treeView1.Refresh();
                    }
                    else
                    {
                        DeleteFromTV(selec, node);

                    }
                }
                fl.RemoveFile(selec);
                
            }
        }
        public void DeleteFromTV(string nodeToDelete, TreeNode treeNode) // delete from treeview
        {
            Form1 frm1 = (Form1)Application.OpenForms["Form1"];

            foreach (TreeNode n in treeNode.Nodes)
                    {
                if (String.IsNullOrEmpty(n.Text))
                {
                    return;
                }
                if (n.Text == nodeToDelete)
                {
                    frm1.treeView1.SelectedNode = n;
                    frm1.treeView1.Nodes.Remove(n);
                    frm1.treeView1.Refresh();

                }
                else DeleteFromTV(nodeToDelete, n);
                    }
                }
            

        
        public void DeleteDir(string dir) // delete from bd
        {   
            string query = "delete from directories where Dir_Name = @rn ";
            MySqlCommand cmd = new MySqlCommand();
            cmd.CommandText = query;
            cmd.Parameters.AddWithValue("@rn", dir);

            cmd.Connection = cr.conn;
            cmd.ExecuteNonQuery();
        }
                }
}
