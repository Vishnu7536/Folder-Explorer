
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace StudentRegistrationForm
{
    public partial class Form1 : Form
    {
        private readonly TreeView folderTree = new TreeView();
        private readonly ListView fileList = new ListView();
        private readonly SplitContainer split = new SplitContainer();
        private readonly StatusStrip status = new StatusStrip();
        private readonly ToolStripStatusLabel statusLabel =
            new ToolStripStatusLabel();

        public Form1()
        {
            Text = "Folder Explorer";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;

            CreateLayout();
            LoadDrives();
        }

        private void CreateLayout()
        {
            // Split Container
            split.Dock = DockStyle.Fill;
            split.SplitterDistance = 300;
            split.FixedPanel = FixedPanel.Panel1;

            // TreeView
            folderTree.Dock = DockStyle.Fill;
            folderTree.Font = new Font("Segoe UI", 10);
            folderTree.HideSelection = false;

            folderTree.BeforeExpand += FolderTree_BeforeExpand;
            folderTree.AfterSelect += FolderTree_AfterSelect;

            split.Panel1.Controls.Add(folderTree);

            // ListView
            fileList.Dock = DockStyle.Fill;
            fileList.View = View.Details;
            fileList.FullRowSelect = true;
            fileList.GridLines = true;
            fileList.MultiSelect = false;
            fileList.HideSelection = false;

            fileList.Columns.Add("Name", 300);
            fileList.Columns.Add("Size", 150);
            fileList.Columns.Add("Date Modified", 200);

            split.Panel2.Controls.Add(fileList);

            // Status Bar
            statusLabel.Text = "Ready";

            status.Items.Add(statusLabel);
            status.Dock = DockStyle.Bottom;

            Controls.Add(split);
            Controls.Add(status);

            split.BringToFront();
            status.BringToFront();
        }

        private void LoadDrives()
        {
            folderTree.Nodes.Clear();

            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    try
                    {
                        if (!drive.IsReady)
                            continue;

                        string drivePath = drive.Name;

                        TreeNode node = new TreeNode(drivePath);
                        node.Tag = drivePath;

                        AddDummyNode(node);

                        folderTree.Nodes.Add(node);
                    }
                    catch (IOException)
                    {
                        // Skip unavailable drives
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip inaccessible drives
                    }
                }

                statusLabel.Text =
                    "Drives loaded: " + folderTree.Nodes.Count;
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Error loading drives";
                MessageBox.Show(
                    "Unable to load drives: " + ex.Message);
            }
        }

        private void AddDummyNode(TreeNode node)
        {
            node.Nodes.Add(new TreeNode("Loading..."));
        }

        private void FolderTree_BeforeExpand(
            object? sender, TreeViewCancelEventArgs e)
        {
            TreeNode node = e.Node;

            if (node == null)
                return;

            // Already loaded
            if (node.Nodes.Count > 0 &&
                node.Nodes[0].Tag != null)
                return;

            node.Nodes.Clear();

            string? folderPath = node.Tag as string;

            if (string.IsNullOrEmpty(folderPath))
                return;

            try
            {
                string[] directories =
                    Directory.GetDirectories(folderPath);

                foreach (string directory in directories)
                {
                    DirectoryInfo info =
                        new DirectoryInfo(directory);

                    TreeNode child = new TreeNode(info.Name);

                    child.Tag = directory;

                    AddDummyNode(child);

                    node.Nodes.Add(child);
                }
            }
            catch (UnauthorizedAccessException)
            {
                statusLabel.Text =
                    "Access denied: " + folderPath;
            }
            catch (IOException)
            {
                statusLabel.Text =
                    "Unable to read: " + folderPath;
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Error: " + ex.Message;
            }
        }

        private void FolderTree_AfterSelect(
            object? sender, TreeViewEventArgs e)
        {
            string? selectedPath = e.Node.Tag as string;

            if (string.IsNullOrEmpty(selectedPath))
                return;

            LoadFiles(selectedPath);
        }

        private void LoadFiles(string folderPath)
        {
            fileList.Items.Clear();

            try
            {
                DirectoryInfo directory =
                    new DirectoryInfo(folderPath);

                // Load Subfolders
                foreach (DirectoryInfo folder in directory.GetDirectories())
                {
                    ListViewItem item =
                        new ListViewItem(folder.Name);

                    item.SubItems.Add("<DIR>");

                    item.SubItems.Add(
                        folder.LastWriteTime.ToString("dd-MM-yyyy HH:mm"));

                    item.Tag = folder.FullName;

                    fileList.Items.Add(item);
                }

                // Load Files
                foreach (FileInfo file in directory.GetFiles())
                {
                    ListViewItem item =
                        new ListViewItem(file.Name);

                    item.SubItems.Add(
                        file.Length.ToString("N0") + " bytes");

                    item.SubItems.Add(
                        file.LastWriteTime.ToString("dd-MM-yyyy HH:mm"));

                    item.Tag = file.FullName;

                    fileList.Items.Add(item);
                }

                statusLabel.Text =
                    "Folder: " + folderPath +
                    " | Total Items: " + fileList.Items.Count;
            }
            catch (UnauthorizedAccessException)
            {
                statusLabel.Text = "Access Denied: " + folderPath;

                MessageBox.Show(
                    "You do not have permission to access this folder.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (IOException ex)
            {
                statusLabel.Text = "Unable to read folder";

                MessageBox.Show(
                    "Folder Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "Error loading folder";

                MessageBox.Show(
                    "Error: " + ex.Message);
            }
        }
    }
}
