using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FNFBot20
{
    public partial class Form1 : Form
    {
        public static List<Thread> currentThreads = new List<Thread>();
        public Bot bot { get; set; }

        public static RichTextBox console { get; set; }
        public static Label watchTime { get; set; }
        
        public static Label offset { get; set; }
        
        public static Panel pnlField { get; set; }

        public static bool Rendering = true;

        public static bool LightShow = false;

        public static int SectionSee = 1;

        public static bool DebugEnabled = false;

        public static Form1 Instance;

        public static Label PlayKeyLabel;
        public static Label OffsetUpKeyLabel;
        public static Label OffsetDownKeyLabel;
        
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        
        public Form1()
        {
            InitializeComponent();
            Instance = this;
            CheckForIllegalCrossThreadCalls = false;
            bot = new Bot();
            console = rchConsole;
            offset = label2;
            watchTime = label1;
            pnlField = pnlPlayField;
            checkBox1.Checked = true;
            chkDebug.Checked = false;
            PlayKeyLabel = lblPlayKey;
            OffsetUpKeyLabel = lblOffsetUpKey;
            OffsetDownKeyLabel = lblOffsetDownKey;
            UpdateKeybindLabels();
            button1.Visible = false;
        }

        public static void WriteToConsole(string text)
        {
            console.Text += "[" + DateTime.Now.ToShortTimeString() + "] " + text + "\n";
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void txtbxDir_Enter(object sender, EventArgs e)
        {
            if (txtbxDir.Text == "FNF Game Directory (ex: C:/Users/user/Documents/FNF)")
                txtbxDir.Text = "";
        }

        private void txtbxDir_Leave(object sender, EventArgs e)
        {
            if (txtbxDir.Text == "")
                txtbxDir.Text = "FNF Game Directory (ex: C:/Users/user/Documents/FNF)";
        }

        private void AddSongsFromRoot(string root)
        {
            if (!Directory.Exists(root))
                return;

            foreach (string s in Directory.GetDirectories(root))
            {
                var children = Directory
                    .GetFiles(s, "*.json")
                    .Select(child => new TreeNode(LeadingPath(child)) { Tag = child })
                    .ToArray();

                if (children.Length == 0)
                    continue;

                var parentNode = new TreeNode(LeadingPath(s), children)
                {
                    Tag = s
                };

                treSngSelect.Nodes.Add(parentNode);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var inputPath = txtbxDir.Text;
            if (!Directory.Exists(inputPath))
            {
                WriteToConsole("Directory does not exist");
                return;
            }
            
            WriteToConsole("Directory found! Retrieving data...");
            treSngSelect.Nodes.Clear();

            try
            {
                string gameDir = inputPath;
                string assetsData = null;
                string modsDir = null;

                string last = Path.GetFileName(gameDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

                if (Directory.Exists(Path.Combine(gameDir, "assets", "data")) || Directory.Exists(Path.Combine(gameDir, "mods")))
                {
                    assetsData = Path.Combine(gameDir, "assets", "data");
                    modsDir = Path.Combine(gameDir, "mods");
                }
                else if (string.Equals(last, "assets", StringComparison.OrdinalIgnoreCase))
                {
                    assetsData = Path.Combine(gameDir, "data");
                    var parent = Directory.GetParent(gameDir);
                    if (parent != null)
                        modsDir = Path.Combine(parent.FullName, "mods");
                }
                else if (string.Equals(last, "mods", StringComparison.OrdinalIgnoreCase))
                {
                    modsDir = gameDir;
                    var parent = Directory.GetParent(gameDir);
                    if (parent != null)
                        assetsData = Path.Combine(parent.FullName, "assets", "data");
                }
                else if (string.Equals(last, "data", StringComparison.OrdinalIgnoreCase))
                {
                    var parent = Directory.GetParent(gameDir);
                    if (parent != null)
                    {
                        var parentName = parent.Name;
                        var grand = Directory.GetParent(parent.FullName);
                        if (grand != null && string.Equals(parentName, "assets", StringComparison.OrdinalIgnoreCase))
                        {
                            assetsData = gameDir;
                            modsDir = Path.Combine(grand.FullName, "mods");
                        }
                        else
                        {
                            var modsParent = Directory.GetParent(parent.FullName);
                            if (modsParent != null && string.Equals(modsParent.Name, "mods", StringComparison.OrdinalIgnoreCase))
                            {
                                modsDir = modsParent.FullName;
                                assetsData = Path.Combine(modsParent.Parent?.FullName ?? "", "assets", "data");
                            }
                        }
                    }
                }
                else
                {
                    assetsData = Path.Combine(gameDir, "data");
                    modsDir = Path.Combine(gameDir, "mods");
                }

                if (!string.IsNullOrEmpty(assetsData))
                    AddSongsFromRoot(assetsData);

                if (!string.IsNullOrEmpty(modsDir) && Directory.Exists(modsDir))
                {
                    var modsDataDirect = Path.Combine(modsDir, "data");
                    AddSongsFromRoot(modsDataDirect);

                    foreach (var modFolder in Directory.GetDirectories(modsDir))
                    {
                        var modData = Path.Combine(modFolder, "data");
                        AddSongsFromRoot(modData);
                    }
                }

                if (treSngSelect.Nodes.Count == 0)
                    WriteToConsole("No songs found in assets or mods.");
            }
            catch (Exception ee)
            {
                WriteToConsole("Failed to retrieve data.\n" + ee);
            }
        }

        private void btnManual_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "FNF Charts (*.json)|*.json|All files (*.*)|*.*";
                if (Directory.Exists(txtbxDir.Text))
                    dlg.InitialDirectory = txtbxDir.Text;

                dlg.Title = "Select a chart (.json)";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var path = dlg.FileName;
                    txtbxDir.Text = Path.GetDirectoryName(path);
                    WriteToConsole("Manual chart selected: " + path);
                    bot.Load(path);
                }
            }
        }
        
        private string LeadingPath(string path) => path.Split('\\', '/').Last();
        
        private void treSngSelect_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                var node = e.Node;
                if (node.Nodes.Count > 0)
                    return;

                var fullPath = node.Tag as string;
                if (string.IsNullOrEmpty(fullPath))
                {
                    WriteToConsole("Failed to select map: no path stored.");
                    return;
                }

                WriteToConsole("Selecting " + node.Text);
                bot.Load(fullPath);
            }
            catch (Exception ee)
            {
                WriteToConsole("Failed to select map.\n" + ee);
            }
        }

        private void pnlTop_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); 
        }

        private void lblVer_MouseDown(object sender, MouseEventArgs e)
        {
             ReleaseCapture();
             SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); 
        }

        private void pnlLogo_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); 
        }

        private void label1_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0); 
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Rendering = checkBox1.Checked;
            pnlField.Controls.Clear();
        }

        private void chkDebug_CheckedChanged(object sender, EventArgs e)
        {
            DebugEnabled = chkDebug.Checked;
            WriteToConsole("Debugging: " + DebugEnabled);
        }

        public void UpdateKeybindLabels()
        {
            if (bot == null || bot.kBot == null)
                return;

            if (PlayKeyLabel != null)
                PlayKeyLabel.Text = "Play: " + bot.kBot.PlayKey;
            if (OffsetUpKeyLabel != null)
                OffsetUpKeyLabel.Text = "Offset+: " + bot.kBot.OffsetUpKey;
            if (OffsetDownKeyLabel != null)
                OffsetDownKeyLabel.Text = "Offset-: " + bot.kBot.OffsetDownKey;
        }

        private void btnBindPlay_Click(object sender, EventArgs e)
        {
            if (bot == null || bot.kBot == null)
                return;

            bot.kBot.BeginBind(KeyBot.BindTarget.Play);
            if (PlayKeyLabel != null)
                PlayKeyLabel.Text = "Play: [press key]";
        }

        private void btnBindOffsetUp_Click(object sender, EventArgs e)
        {
            if (bot == null || bot.kBot == null)
                return;

            bot.kBot.BeginBind(KeyBot.BindTarget.OffsetUp);
            if (OffsetUpKeyLabel != null)
                OffsetUpKeyLabel.Text = "Offset+: [press key]";
        }

        private void btnBindOffsetDown_Click(object sender, EventArgs e)
        {
            if (bot == null || bot.kBot == null)
                return;

            bot.kBot.BeginBind(KeyBot.BindTarget.OffsetDown);
            if (OffsetDownKeyLabel != null)
                OffsetDownKeyLabel.Text = "Offset-: [press key]";
        }

        private void btnResetOffset_Click(object sender, EventArgs e)
        {
            if (bot == null || bot.kBot == null)
                return;

            bot.kBot.ResetOffset();
        }
    }
}
