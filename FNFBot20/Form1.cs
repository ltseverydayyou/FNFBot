using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            bot = new Bot();
            console = rchConsole;
            offset = label2;
            watchTime = label1;
            pnlField = pnlPlayField;
            checkBox1.Checked = true;
        }


        public static void WriteToConsole(string text)
        {
            console.Text += "[" + DateTime.Now.ToShortTimeString() + "] " + text + "\n";
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            foreach(Thread t in currentThreads)
                t.Abort();
            bot.kBot.StopHooks();
            Environment.Exit(0);
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
            if (!Directory.Exists(txtbxDir.Text))
            {
                Form1.WriteToConsole("Directory does not exist");
                return;
            }
            
            Form1.WriteToConsole("Directory found! Retrieving data...");
            treSngSelect.Nodes.Clear();

            try
            {
                var gameDir = txtbxDir.Text;

                var assetsData = Path.Combine(gameDir, "assets", "data");
                AddSongsFromRoot(assetsData);

                var modsDir = Path.Combine(gameDir, "mods");
                if (Directory.Exists(modsDir))
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
        
        private string LeadingPath(string path) => path.Split('\\').Last();
        
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

    }
}
