using System;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;

namespace Calculator
{
    /// <summary>
    /// Two-panel font picker: UI font + Editor font, with live preview.
    /// </summary>
    public class FontPickerForm : Form
    {
        public Font SelectedUIFont     { get; private set; }
        public Font SelectedEditorFont { get; private set; }

        // ── UI font pickers ────────────────────────────────
        private ListBox lbUIFont;
        private ComboBox cbUISize;
        private Label lblUIPreview;

        // ── Editor font pickers ────────────────────────────
        private ListBox lbEdFont;
        private ComboBox cbEdSize;
        private Label lblEdPreview;

        // ── Buttons ────────────────────────────────────────
        private Button btnOk, btnCancel;

        private static readonly string[] PreviewText =
        {
            "AaBbCcDd 123",
            "sin(x) + cos(y)"
        };

        private static readonly float[] CommonSizes =
            { 7f, 8f, 8.5f, 9f, 10f, 11f, 12f, 13f, 14f, 16f, 18f, 20f };

        // Mono-spaced font families (heuristic list)
        private static readonly string[] MonoFamilies =
        {
            "Cascadia Code", "Cascadia Mono", "Consolas", "Courier New", "Lucida Console",
            "DejaVu Sans Mono", "Fira Code", "JetBrains Mono", "Source Code Pro",
            "Inconsolata", "Hack", "Roboto Mono", "Noto Mono", "Menlo",
            "Monaco", "Andale Mono", "Ubuntu Mono"
        };

        public FontPickerForm(Font currentUI, Font currentEditor)
        {
            SelectedUIFont     = currentUI;
            SelectedEditorFont = currentEditor;
            BuildUI(currentUI, currentEditor);
        }

        private void BuildUI(Font curUI, Font curEd)
        {
            this.Text            = "Font Settings";
            this.Size            = new Size(700, 560);
            this.MinimumSize     = new Size(600, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.BackColor       = Color.FromArgb(28, 28, 38);
            this.ForeColor       = Color.FromArgb(210, 210, 245);
            this.Font            = curUI;

            // ── Get font families ──────────────────────────
            var installed = new InstalledFontCollection();
            var allFamilies    = installed.Families.Select(f => f.Name).OrderBy(n => n).ToArray();
            var monoFamilies   = allFamilies
                .Where(n => MonoFamilies.Any(m => n.Contains(m, StringComparison.OrdinalIgnoreCase))
                         || n.Contains("mono", StringComparison.OrdinalIgnoreCase)
                         || n.Contains("code",  StringComparison.OrdinalIgnoreCase))
                .ToArray();
            // Fall back to all fonts if list is too short
            if (monoFamilies.Length < 3) monoFamilies = allFamilies;

            // 변경 후 — 에디터도 전체 폰트 목록 사용
            monoFamilies = allFamilies;

            // ── Outer split ───────────────────────────────
            var split = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 2,
                BackColor   = Color.Transparent,
                Padding     = new Padding(12, 12, 12, 8)
            };
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            split.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            split.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // ── Left panel: UI font ───────────────────────
            var pnlUI  = MakePanel("UI Font  (Toolbar, Labels)");
            lbUIFont   = MakeListBox(allFamilies);
            cbUISize   = MakeSizeCombo();
            lblUIPreview = MakePreview();

            lbUIFont.SelectedItem  = curUI.Name;
            cbUISize.SelectedItem  = curUI.Size.ToString("0.#");
            if (cbUISize.SelectedIndex < 0) cbUISize.Text = curUI.Size.ToString("0.#");

            pnlUI.Controls.Add(MakeLabel("Family"));
            pnlUI.Controls.Add(lbUIFont);
            pnlUI.Controls.Add(MakeLabel("Size"));
            pnlUI.Controls.Add(cbUISize);
            pnlUI.Controls.Add(MakeLabel("Preview"));
            pnlUI.Controls.Add(lblUIPreview);

            // ── Right panel: Editor font ──────────────────
            var pnlEd  = MakePanel("Editor Font  (Input / Output)");
            lbEdFont   = MakeListBox(monoFamilies.Length >= 3 ? monoFamilies : allFamilies);
            cbEdSize   = MakeSizeCombo();
            lblEdPreview = MakePreview();

            lbEdFont.SelectedItem = curEd.Name;
            cbEdSize.SelectedItem = curEd.Size.ToString("0.#");
            if (cbEdSize.SelectedIndex < 0) cbEdSize.Text = curEd.Size.ToString("0.#");

            pnlEd.Controls.Add(MakeLabel("Family"));
            pnlEd.Controls.Add(lbEdFont);
            pnlEd.Controls.Add(MakeLabel("Size"));
            pnlEd.Controls.Add(cbEdSize);
            pnlEd.Controls.Add(MakeLabel("Preview"));
            pnlEd.Controls.Add(lblEdPreview);

            // ── Button row ───────────────────────────────
            var btnPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                BackColor     = Color.Transparent,
                Padding       = new Padding(0, 6, 0, 0)
            };
            btnOk = MakeDialogBtn("  Apply  ", true);
            btnCancel = MakeDialogBtn("Cancel", false);
            btnPanel.Controls.Add(btnOk);
            btnPanel.Controls.Add(btnCancel);

            split.Controls.Add(pnlUI,    0, 0);
            split.Controls.Add(pnlEd,    1, 0);
            split.Controls.Add(btnPanel, 0, 1);
            split.SetColumnSpan(btnPanel, 2);

            this.Controls.Add(split);

            // ── Wire events ──────────────────────────────
            lbUIFont.SelectedIndexChanged += (s, e) => UpdateUIPreview();
            cbUISize.SelectedIndexChanged += (s, e) => UpdateUIPreview();
            cbUISize.TextChanged          += (s, e) => UpdateUIPreview();

            lbEdFont.SelectedIndexChanged += (s, e) => UpdateEdPreview();
            cbEdSize.SelectedIndexChanged += (s, e) => UpdateEdPreview();
            cbEdSize.TextChanged          += (s, e) => UpdateEdPreview();

            btnOk.Click     += BtnOk_Click;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; Close(); };

            UpdateUIPreview();
            UpdateEdPreview();
        }

        private void UpdateUIPreview()
        {
            var f = BuildFont(lbUIFont, cbUISize, 9f);
            lblUIPreview.Font = f;
            lblUIPreview.Text = string.Join("  ·  ", PreviewText) + $"\n{f.Name}, {f.Size}pt";
        }

        private void UpdateEdPreview()
        {
            var f = BuildFont(lbEdFont, cbEdSize, 11f);
            lblEdPreview.Font = f;
            lblEdPreview.Text = string.Join("  ·  ", PreviewText) + $"\n{f.Name}, {f.Size}pt";
        }

        private Font BuildFont(ListBox lb, ComboBox cb, float fallbackSize)
        {
            string name = lb.SelectedItem?.ToString() ?? "Courier New";
            float  size = fallbackSize;
            if (float.TryParse(cb.Text, out float s) && s >= 6 && s <= 36) size = s;
            try { return new Font(name, size); }
            catch { return new Font("Courier New", size); }
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            SelectedUIFont     = BuildFont(lbUIFont, cbUISize, 9f);
            SelectedEditorFont = BuildFont(lbEdFont, cbEdSize, 11f);
            this.DialogResult  = DialogResult.OK;
            Close();
        }

        // ── Helpers ──────────────────────────────────────
        private Panel MakePanel(string title)
        {
            var p = new Panel
            {
                Dock      = DockStyle.Fill,
                Margin    = new Padding(4),
                BackColor = Color.FromArgb(34, 34, 46)
            };
            p.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(
                    new Pen(Color.FromArgb(60, 60, 90), 1), 0, 0, p.Width - 1, p.Height - 1);
                using var hf = new Font("Segoe UI", 8.5f, FontStyle.Bold);
                using var hb = new SolidBrush(Color.FromArgb(130, 120, 240));
                e.Graphics.DrawString(title, hf, hb, 8, 6);
            };
            p.Layout += (s, e) => LayoutPanel((Panel)s!);
            return p;
        }

        private void LayoutPanel(Panel p)
        {
            var controls = p.Controls.Cast<Control>().ToList();
            int y = 28, pad = 8;
            foreach (var c in controls)
            {
                c.Left  = pad;
                c.Width = p.Width - pad * 2;
                c.Top   = y;
                y += c.Height + 4;
            }
        }

        private Label MakeLabel(string text) => new Label
        {
            Text      = text,
            AutoSize  = false,
            Height    = 18,
            ForeColor = Color.FromArgb(150, 150, 195),
            Font      = new Font("Segoe UI", 7.5f)
        };

        private ListBox MakeListBox(string[] items)
        {
            var lb = new ListBox
            {
                Height          = 130,
                BackColor       = Color.FromArgb(22, 22, 32),
                ForeColor       = Color.FromArgb(210, 210, 245),
                BorderStyle     = BorderStyle.FixedSingle,
                IntegralHeight  = false,
                ScrollAlwaysVisible = false
            };
            lb.Items.AddRange(items);
            return lb;
        }

        private ComboBox MakeSizeCombo()
        {
            var cb = new ComboBox
            {
                Height      = 24,
                DropDownStyle = ComboBoxStyle.DropDown,
                BackColor   = Color.FromArgb(22, 22, 32),
                ForeColor   = Color.FromArgb(210, 210, 245),
                FlatStyle   = FlatStyle.Flat
            };
            foreach (var s in CommonSizes) cb.Items.Add(s.ToString("0.#"));
            return cb;
        }

        private Label MakePreview() => new Label
        {
            Height    = 56,
            BackColor = Color.FromArgb(18, 18, 28),
            ForeColor = Color.FromArgb(210, 210, 245),
            BorderStyle = BorderStyle.FixedSingle,
            AutoSize  = false,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding   = new Padding(6, 0, 0, 0)
        };

        private Button MakeDialogBtn(string text, bool isDefault)
        {
            var b = new Button
            {
                Text         = text,
                Width        = 90,
                Height       = 32,
                FlatStyle    = FlatStyle.Flat,
                BackColor    = isDefault ? Color.FromArgb(90, 75, 200) : Color.FromArgb(50, 50, 70),
                ForeColor    = Color.White,
                Cursor       = Cursors.Hand,
                DialogResult = isDefault ? DialogResult.None : DialogResult.Cancel,
                Margin       = new Padding(4, 0, 0, 0)
            };
            b.FlatAppearance.BorderSize = 0;
            if (isDefault) this.AcceptButton = b;
            else           this.CancelButton = b;
            return b;
        }
    }
}
