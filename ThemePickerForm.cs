using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Calculator
{
    public class ThemePickerForm : Form
    {
        public AppTheme SelectedTheme { get; private set; }

        private Button btnOk, btnCancel;

        private static readonly (AppTheme theme, string name, string desc,
            Color bg, Color panel, Color editor, Color fg, Color accent)[] Themes =
        {
            (AppTheme.Dark,    "Dark",    "Deep purple-dark",
             Color.FromArgb(18,18,26),   Color.FromArgb(26,26,36),
             Color.FromArgb(20,20,28),   Color.FromArgb(215,215,255),
             Color.FromArgb(100,80,215)),

            (AppTheme.Light,   "Light",   "Clean white",
             Color.FromArgb(242,242,250), Color.FromArgb(230,230,244),
             Color.FromArgb(255,255,255), Color.FromArgb(25,25,55),
             Color.FromArgb(85,65,195)),

            (AppTheme.Monokai, "Monokai", "Retro green-teal",
             Color.FromArgb(39,40,34),   Color.FromArgb(50,51,44),
             Color.FromArgb(39,40,34),   Color.FromArgb(248,248,242),
             Color.FromArgb(81,193,194)),
        };

        public ThemePickerForm(AppTheme current, Font uiFont)
        {
            SelectedTheme        = current;
            this.Text            = "Choose Theme";
            this.Size            = new Size(560, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.FromArgb(28, 28, 38);
            this.ForeColor       = Color.FromArgb(210, 210, 245);
            this.Font            = uiFont;

            var flow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Padding       = new Padding(16, 16, 16, 0),
                BackColor     = Color.Transparent,
                AutoScroll    = false
            };

            foreach (var t in Themes)
            {
                var tile = MakeTile(t.theme, t.name, t.desc,
                                    t.bg, t.panel, t.editor, t.fg, t.accent,
                                    t.theme == current);
                flow.Controls.Add(tile);
            }

            var btnRow = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Color.FromArgb(22, 22, 32),
                Padding   = new Padding(0, 10, 16, 0)
            };

            btnOk = new Button
            {
                Text      = "Apply",
                Size      = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(90, 75, 200),
                ForeColor = Color.White,
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            btnOk.FlatAppearance.BorderSize = 0;
            btnOk.Location = new Point(btnRow.Width - 200, 10);
            btnOk.Anchor   = AnchorStyles.Top | AnchorStyles.Right;

            btnCancel = new Button
            {
                Text      = "Cancel",
                Size      = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 70),
                ForeColor = Color.White,
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Location = new Point(btnRow.Width - 100, 10);
            btnCancel.Anchor   = AnchorStyles.Top | AnchorStyles.Right;

            btnRow.Controls.Add(btnOk);
            btnRow.Controls.Add(btnCancel);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            this.Controls.Add(flow);
            this.Controls.Add(btnRow);

            btnOk.Click     += (s, e) => { this.DialogResult = DialogResult.OK;     Close(); };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; Close(); };
        }

        private Control MakeTile(AppTheme theme, string name, string desc,
            Color bg, Color panel, Color editor, Color fg, Color accent, bool selected)
        {
            var tile = new Panel
            {
                Size      = new Size(154, 190),
                Margin    = new Padding(4),
                BackColor = panel,
                Cursor    = Cursors.Hand,
                Tag       = theme
            };

            // Selection ring
            bool isSelected = selected;
            tile.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                // Fake editor area
                using (var b = new SolidBrush(editor))
                    g.FillRectangle(b, 8, 30, tile.Width - 16, 80);
                // Fake lines
                using var lb = new SolidBrush(Color.FromArgb(60, fg));
                g.FillRectangle(lb, 14, 40, 80, 7);
                g.FillRectangle(lb, 14, 54, 110, 7);
                g.FillRectangle(lb, 14, 68, 65, 7);
                g.FillRectangle(lb, 14, 82, 90, 7);
                // Accent bar
                using var ab = new SolidBrush(accent);
                g.FillRectangle(ab, 8, 116, tile.Width - 16, 4);
                // Fake button
                using var rp = RoundedPath(new Rectangle(8, 126, 60, 18), 4);
                g.FillPath(ab, rp);
                // Name label
                using var nf = new Font("Segoe UI", 9f, FontStyle.Bold);
                using var nb = new SolidBrush(fg);
                g.DrawString(name, nf, nb, 8, 150);
                // Desc
                using var df = new Font("Segoe UI", 7f);
                using var db = new SolidBrush(Color.FromArgb(160, fg));
                g.DrawString(desc, df, db, 8, 167);
                // Selection border
                if (isSelected)
                    g.DrawRectangle(new Pen(accent, 2.5f), 1, 1, tile.Width - 3, tile.Height - 3);
            };

            tile.Click += (s, e) =>
            {
                SelectedTheme = theme;
                isSelected = true;
                // Deselect others
                var parent = tile.Parent;
                if (parent != null)
                    foreach (Control c in parent.Controls)
                        if (c != tile) c.Invalidate();
                tile.Invalidate();
            };

            return tile;
        }

        private static GraphicsPath RoundedPath(Rectangle r, int rad)
        {
            var p = new GraphicsPath(); int d = rad * 2;
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
    }
}
