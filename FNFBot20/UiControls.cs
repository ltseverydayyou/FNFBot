using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FNFBot20
{
    public class RoundedPanel : Panel
    {
        public int CornerRadius { get; set; } = 12;
        public Color BorderColor { get; set; } = Color.Transparent;
        public int BorderThickness { get; set; } = 1;

        public RoundedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateRegion();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (BorderThickness <= 0 || BorderColor == Color.Transparent)
                return;

            using (var path = CreatePath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius))
            using (var pen = new Pen(BorderColor, BorderThickness))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            }
        }

        private void UpdateRegion()
        {
            if (Width <= 1 || Height <= 1)
                return;

            using (var path = CreatePath(new Rectangle(0, 0, Width, Height), CornerRadius))
                Region = new Region(path);
        }

        internal static GraphicsPath CreatePath(Rectangle bounds, int radius)
        {
            int diameter = Math.Max(2, radius * 2);
            int right = bounds.Right - diameter;
            int bottom = bounds.Bottom - diameter;
            var path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(right, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(right, bottom, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bottom, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class AnimatedButton : Button
    {
        private readonly Timer hoverTimer;
        private Color currentColor;
        private Color targetColor;

        public int CornerRadius { get; set; } = 8;
        public Color NormalColor { get; set; } = Color.FromArgb(35, 27, 58);
        public Color HoverColor { get; set; } = Color.FromArgb(65, 48, 100);
        public Color PressedColor { get; set; } = Color.FromArgb(86, 61, 135);

        public AnimatedButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            UseVisualStyleBackColor = false;
            Cursor = Cursors.Hand;
            currentColor = NormalColor;
            targetColor = NormalColor;
            BackColor = currentColor;

            hoverTimer = new Timer { Interval = 15 };
            hoverTimer.Tick += (sender, args) => AnimateColor();

            MouseEnter += (sender, args) => SetTarget(HoverColor);
            MouseLeave += (sender, args) => SetTarget(NormalColor);
            MouseDown += (sender, args) => SetTarget(PressedColor);
            MouseUp += (sender, args) => SetTarget(ClientRectangle.Contains(PointToClient(Cursor.Position)) ? HoverColor : NormalColor);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRegion();
        }

        private void SetTarget(Color color)
        {
            targetColor = color;
            if (!hoverTimer.Enabled)
                hoverTimer.Start();
        }

        private void AnimateColor()
        {
            currentColor = Color.FromArgb(
                Step(currentColor.A, targetColor.A),
                Step(currentColor.R, targetColor.R),
                Step(currentColor.G, targetColor.G),
                Step(currentColor.B, targetColor.B)
            );
            BackColor = currentColor;

            if (currentColor.ToArgb() == targetColor.ToArgb())
                hoverTimer.Stop();
        }

        private int Step(int current, int target)
        {
            if (current == target)
                return current;
            int delta = target - current;
            int amount = Math.Max(1, Math.Abs(delta) / 4);
            return current + Math.Sign(delta) * Math.Min(Math.Abs(delta), amount);
        }

        private void UpdateRegion()
        {
            if (Width <= 1 || Height <= 1)
                return;

            using (var path = RoundedPanel.CreatePath(new Rectangle(0, 0, Width, Height), CornerRadius))
                Region = new Region(path);
        }
    }

    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }
}
