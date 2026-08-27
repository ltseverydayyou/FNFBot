using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FNFBot20
{
    public partial class Form1 : Form
    {
        public Bot bot { get; set; }

        public static RichTextBox console { get; set; }
        public static Label watchTime { get; set; }
        public static Label offset { get; set; }
        public static Panel pnlField { get; set; }

        public static bool Rendering = true;
        public static bool LightShow = false;
        public static int SectionSee = 1;

        public static Form1 Instance;
        public static Label PlayKeyLabel;
        public static Label OffsetUpKeyLabel;
        public static Label OffsetDownKeyLabel;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private int navTargetTop;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int left, int top, int right, int bottom, int widthEllipse, int heightEllipse);

        public Form1()
        {
            InitializeComponent();
            Instance = this;
            console = rchConsole;
            offset = label2;
            watchTime = label1;
            pnlField = pnlPlayField;
            PlayKeyLabel = lblPlayKey;
            OffsetUpKeyLabel = lblOffsetUpKey;
            OffsetDownKeyLabel = lblOffsetDownKey;

            bot = new Bot();
            UpdateKeybindLabels();

            checkBox1.Checked = true;
            for (int i = 1; i <= 18; i++)
                cmbLaneCount.Items.Add(i + "K");
            cmbLaneCount.SelectedIndex = 3;

            navTargetTop = navDashboard.Top;
            navIndicator.Top = navTargetTop;
            ShowPage(pnlDashboardPage, navDashboard);
            RefreshLaneKeyButtons();
            UpdateFormRegion();
        }

        public static void WriteToConsole(string text)
        {
            if (console == null || console.IsDisposed)
                return;

            Action write = () =>
            {
                if (console == null || console.IsDisposed)
                    return;
                console.AppendText("[" + DateTime.Now.ToShortTimeString() + "] " + text + Environment.NewLine);
                console.SelectionStart = console.TextLength;
                console.ScrollToCaret();
            };

            try
            {
                if (console.InvokeRequired)
                    console.BeginInvoke(write);
                else
                    write();
            }
            catch
            {
            }
        }

        private void ShowPage(Control page, AnimatedButton activeButton)
        {
            pnlDashboardPage.Visible = page == pnlDashboardPage;
            pnlKeybindPage.Visible = page == pnlKeybindPage;
            page.BringToFront();

            navDashboard.NormalColor = Color.FromArgb(24, 18, 41);
            navKeybinds.NormalColor = Color.FromArgb(24, 18, 41);
            activeButton.NormalColor = Color.FromArgb(49, 35, 78);
            activeButton.BackColor = activeButton.NormalColor;

            navTargetTop = activeButton.Top;
            if (!navAnimationTimer.Enabled)
                navAnimationTimer.Start();
        }

        private void navAnimationTimer_Tick(object sender, EventArgs e)
        {
            int delta = navTargetTop - navIndicator.Top;
            if (delta == 0)
            {
                navAnimationTimer.Stop();
                return;
            }

            int step = Math.Max(1, Math.Abs(delta) / 3);
            navIndicator.Top += Math.Sign(delta) * Math.Min(Math.Abs(delta), step);
        }

        private void navDashboard_Click(object sender, EventArgs e)
        {
            ShowPage(pnlDashboardPage, navDashboard);
        }

        private void navKeybinds_Click(object sender, EventArgs e)
        {
            ShowPage(pnlKeybindPage, navKeybinds);
            RefreshLaneKeyButtons();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void txtbxDir_Enter(object sender, EventArgs e)
        {
            if (txtbxDir.Text == "FNF Game Directory (ex: C:/Users/user/Documents/FNF)")
                txtbxDir.Text = "";
        }

        private void txtbxDir_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtbxDir.Text))
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

                treSngSelect.Nodes.Add(new TreeNode(LeadingPath(s), children) { Tag = s });
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string inputPath = txtbxDir.Text;
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
                    DirectoryInfo parent = Directory.GetParent(gameDir);
                    if (parent != null)
                        modsDir = Path.Combine(parent.FullName, "mods");
                }
                else if (string.Equals(last, "mods", StringComparison.OrdinalIgnoreCase))
                {
                    modsDir = gameDir;
                    DirectoryInfo parent = Directory.GetParent(gameDir);
                    if (parent != null)
                        assetsData = Path.Combine(parent.FullName, "assets", "data");
                }
                else if (string.Equals(last, "data", StringComparison.OrdinalIgnoreCase))
                {
                    DirectoryInfo parent = Directory.GetParent(gameDir);
                    if (parent != null)
                    {
                        DirectoryInfo grand = Directory.GetParent(parent.FullName);
                        if (grand != null && string.Equals(parent.Name, "assets", StringComparison.OrdinalIgnoreCase))
                        {
                            assetsData = gameDir;
                            modsDir = Path.Combine(grand.FullName, "mods");
                        }
                        else
                        {
                            DirectoryInfo modsParent = Directory.GetParent(parent.FullName);
                            if (modsParent != null && string.Equals(modsParent.Name, "mods", StringComparison.OrdinalIgnoreCase))
                            {
                                modsDir = modsParent.FullName;
                                assetsData = Path.Combine(modsParent.Parent != null ? modsParent.Parent.FullName : "", "assets", "data");
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
                    AddSongsFromRoot(Path.Combine(modsDir, "data"));
                    foreach (string modFolder in Directory.GetDirectories(modsDir))
                        AddSongsFromRoot(Path.Combine(modFolder, "data"));
                }

                if (treSngSelect.Nodes.Count == 0)
                    WriteToConsole("No songs found in assets or mods.");
                else
                    WriteToConsole("Found " + treSngSelect.Nodes.Count + " song folders.");
            }
            catch (Exception exception)
            {
                WriteToConsole("Failed to retrieve data.\n" + exception);
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

                if (dlg.ShowDialog() != DialogResult.OK)
                    return;

                txtbxDir.Text = Path.GetDirectoryName(dlg.FileName);
                WriteToConsole("Manual chart selected: " + dlg.FileName);
                bot.Load(dlg.FileName);
                if (bot.mBot != null)
                    SetDetectedKeyCount(bot.mBot.KeyCount);
            }
        }

        private string LeadingPath(string path)
        {
            return path.Split('\\', '/').Last();
        }

        private void treSngSelect_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                TreeNode node = e.Node;
                if (node.Nodes.Count > 0)
                    return;

                string fullPath = node.Tag as string;
                if (string.IsNullOrEmpty(fullPath))
                {
                    WriteToConsole("Failed to select map: no path stored.");
                    return;
                }

                WriteToConsole("Selecting " + node.Text);
                bot.Load(fullPath);
                if (bot.mBot != null)
                    SetDetectedKeyCount(bot.mBot.KeyCount);
            }
            catch (Exception exception)
            {
                WriteToConsole("Failed to select map.\n" + exception);
            }
        }

        private void pnlTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            ReleaseCapture();
            SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Rendering = checkBox1.Checked;
            pnlField.Controls.Clear();
        }

        public void UpdateKeybindLabels()
        {
            if (bot == null || bot.kBot == null)
                return;

            if (PlayKeyLabel != null)
                PlayKeyLabel.Text = FriendlyKeyName(bot.kBot.PlayKey);
            if (OffsetUpKeyLabel != null)
                OffsetUpKeyLabel.Text = FriendlyKeyName(bot.kBot.OffsetUpKey);
            if (OffsetDownKeyLabel != null)
                OffsetDownKeyLabel.Text = FriendlyKeyName(bot.kBot.OffsetDownKey);
        }

        private void btnBindPlay_Click(object sender, EventArgs e)
        {
            bot.kBot.BeginBind(KeyBot.BindTarget.Play);
            lblPlayKey.Text = "Press a key...";
        }

        private void btnBindOffsetUp_Click(object sender, EventArgs e)
        {
            bot.kBot.BeginBind(KeyBot.BindTarget.OffsetUp);
            lblOffsetUpKey.Text = "Press a key...";
        }

        private void btnBindOffsetDown_Click(object sender, EventArgs e)
        {
            bot.kBot.BeginBind(KeyBot.BindTarget.OffsetDown);
            lblOffsetDownKey.Text = "Press a key...";
        }

        private void btnResetShortcuts_Click(object sender, EventArgs e)
        {
            bot.kBot.ResetKeybinds();
            UpdateKeybindLabels();
            WriteToConsole("Reset bot shortcuts to F1 / F2 / F3.");
        }

        private void btnResetOffset_Click(object sender, EventArgs e)
        {
            bot.kBot.ResetOffset();
        }

        private void cmbLaneCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshLaneKeyButtons();
        }

        public void SetDetectedKeyCount(int keyCount)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)(() => SetDetectedKeyCount(keyCount)));
                return;
            }

            int clamped = Math.Max(1, Math.Min(18, keyCount));
            cmbLaneCount.SelectedIndex = clamped - 1;
            lblDetectedLayout.Text = "Detected layout: " + clamped + "K";
        }

        public void RefreshLaneKeyButtons()
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)RefreshLaneKeyButtons);
                return;
            }

            if (bot == null || cmbLaneCount.SelectedIndex < 0)
                return;

            int keyCount = cmbLaneCount.SelectedIndex + 1;
            int[] layout = bot.GetLaneLayout(keyCount);
            flpLaneKeys.SuspendLayout();
            flpLaneKeys.Controls.Clear();

            for (int i = 0; i < layout.Length; i++)
            {
                int lane = i;
                var button = new AnimatedButton
                {
                    Width = 132,
                    Height = 58,
                    Margin = new Padding(0, 0, 10, 10),
                    CornerRadius = 8,
                    Font = new Font("Segoe UI Semibold", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(239, 235, 250),
                    NormalColor = Color.FromArgb(36, 28, 58),
                    HoverColor = Color.FromArgb(62, 45, 96),
                    PressedColor = Color.FromArgb(91, 62, 145),
                    Text = "LANE " + (lane + 1) + Environment.NewLine + FriendlyKeyName((Keys)layout[lane]),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Tag = lane
                };

                button.Click += (sender, args) =>
                {
                    bot.kBot.BeginLaneBind(keyCount, lane);
                    button.Text = "LANE " + (lane + 1) + Environment.NewLine + "Press a key...";
                };

                flpLaneKeys.Controls.Add(button);
            }

            lblLaneProfile.Text = keyCount + "K key profile";
            flpLaneKeys.ResumeLayout();
        }

        private void btnResetLaneKeys_Click(object sender, EventArgs e)
        {
            if (cmbLaneCount.SelectedIndex < 0)
                return;
            int keyCount = cmbLaneCount.SelectedIndex + 1;
            bot.ResetLaneLayout(keyCount);
            RefreshLaneKeyButtons();
            WriteToConsole("Reset " + keyCount + "K lane keybinds to defaults.");
        }

        private void btnResetAllLaneKeys_Click(object sender, EventArgs e)
        {
            bot.ResetAllLaneLayouts();
            RefreshLaneKeyButtons();
            WriteToConsole("Reset all lane keybind profiles to defaults.");
        }

        private string FriendlyKeyName(Keys key)
        {
            switch (key)
            {
                case Keys.Left: return "Left Arrow";
                case Keys.Right: return "Right Arrow";
                case Keys.Up: return "Up Arrow";
                case Keys.Down: return "Down Arrow";
                case Keys.Space: return "Space";
                case Keys.OemPeriod: return ".";
                case Keys.Oemcomma: return ",";
                default: return key.ToString();
            }
        }

        private void UpdateFormRegion()
        {
            if (WindowState == FormWindowState.Maximized || Width <= 0 || Height <= 0)
            {
                Region = null;
                return;
            }

            IntPtr regionHandle = CreateRoundRectRgn(0, 0, Width + 1, Height + 1, 18, 18);
            Region = Region.FromHrgn(regionHandle);
            DeleteObject(regionHandle);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            UpdateFormRegion();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!e.Cancel && bot != null)
                bot.Shutdown();
        }
    }
}
