using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace Calculator
{
    public class CalculatorForm : Form
    {
        // ── Controls ──────────────────────────────────────────
        private RichTextBox rtbMain;
        private RadioButton rbDeg, rbRad;
        private RadioButton rbDec, rbHex, rbBin;
        private CheckBox cbEcho;
        private ModernButton btnVar, btnHelp, btnFont, btnTheme;
        private Panel topPanel;
        private StatusStrip statusBar;
        private ToolStripStatusLabel lblStatus;

        // ── State ──────────────────────────────────────────────
        private Dictionary<string, double> variables = new(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, System.Numerics.Complex> complexVars = new(StringComparer.OrdinalIgnoreCase);
        private string lastResult = "";
        private int outputPrecision = 10;

        // ── Settings ───────────────────────────────────────────
        private AppSettings _settings = new();

        // ── Theme ──────────────────────────────────────────────
        private AppTheme _theme = AppTheme.Light;

        private Color clrBackground, clrTopPanel, clrEditor, clrEditorFg;
        private Color clrAccent, clrAccentHov, clrRadioFg, clrStatusBg, clrStatusFg;

        // ── Fonts ──────────────────────────────────────────────
        private Font _uiFont     = TryFont("Segoe UI", 9f) ?? new Font("Arial", 9f);
        private Font _editorFont = TryFont("Cascadia Code", 11f)
                                ?? TryFont("Consolas", 11f)
                                ?? new Font("Courier New", 11f);

        public CalculatorForm()
        {
            //this.Icon = new Icon("Calculator.ico");

            // ── 설정 불러오기 ──────────────────────────────────
            _settings    = AppSettings.Load();
            _theme       = _settings.GetTheme();
            _uiFont      = _settings.GetUIFont();
            _editorFont  = _settings.GetEditorFont();

            SetThemeColors();
            InitializeComponent();
            ApplyThemeToControls();
            variables["pi"] = Math.PI;
            variables["e"]  = Math.E;
            SetStatus("Ready  —  F1: Help   Enter: Calculate");
        }

        // ══════════════════════════════════════════════════════
        //  Font helper
        // ══════════════════════════════════════════════════════
        private static Font? TryFont(string name, float size)
        {
            try
            {
                var f = new Font(name, size);
                return f.Name == name ? f : null;
            }
            catch { return null; }
        }

        // ══════════════════════════════════════════════════════
        //  Theme colors
        // ══════════════════════════════════════════════════════
        private void SetThemeColors()
        {
            switch (_theme)
            {
                case AppTheme.Dark:
                    clrBackground = Color.FromArgb(18, 18, 26);
                    clrTopPanel   = Color.FromArgb(26, 26, 36);
                    clrEditor     = Color.FromArgb(20, 20, 28);
                    clrEditorFg   = Color.FromArgb(215, 215, 255);
                    clrAccent     = Color.FromArgb(100, 80, 215);
                    clrAccentHov  = Color.FromArgb(130, 110, 245);
                    clrRadioFg    = Color.FromArgb(170, 170, 210);
                    clrStatusBg   = Color.FromArgb(22, 22, 32);
                    clrStatusFg   = Color.FromArgb(100, 100, 150);
                    break;
                case AppTheme.Light:
                    clrBackground = Color.FromArgb(242, 242, 250);
                    clrTopPanel   = Color.FromArgb(230, 230, 244);
                    clrEditor     = Color.FromArgb(255, 255, 255);
                    clrEditorFg   = Color.FromArgb(25, 25, 55);
                    clrAccent     = Color.FromArgb(85, 65, 195);
                    clrAccentHov  = Color.FromArgb(105, 85, 215);
                    clrRadioFg    = Color.FromArgb(55, 55, 100);
                    clrStatusBg   = Color.FromArgb(220, 220, 238);
                    clrStatusFg   = Color.FromArgb(90, 90, 140);
                    break;
                case AppTheme.Monokai:
                    clrBackground = Color.FromArgb(39, 40, 34);
                    clrTopPanel   = Color.FromArgb(50, 51, 44);
                    clrEditor     = Color.FromArgb(39, 40, 34);
                    clrEditorFg   = Color.FromArgb(248, 248, 242);
                    clrAccent     = Color.FromArgb(81, 193, 194);
                    clrAccentHov  = Color.FromArgb(110, 220, 220);
                    clrRadioFg    = Color.FromArgb(200, 200, 180);
                    clrStatusBg   = Color.FromArgb(30, 31, 26);
                    clrStatusFg   = Color.FromArgb(117, 113, 94);
                    break;
            }
        }

        // ══════════════════════════════════════════════════════
        //  Build UI
        // ══════════════════════════════════════════════════════
        private void InitializeComponent()
        {
            this.Text        = "Calculator";
            this.Size        = new Size(820, 640);
            this.MinimumSize = new Size(540, 420);
            this.BackColor   = clrBackground;
            this.Font        = _uiFont;
            this.KeyPreview  = true;

            // ── Top panel ─────────────────────────────────────
            topPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 46,
                BackColor = clrTopPanel,
                Padding   = new Padding(8, 0, 8, 0)
            };
            topPanel.Paint += (s, e) =>
            {
                int y = topPanel.Height - 1;
                using var pen = new Pen(clrAccent);
                e.Graphics.DrawLine(pen, 0, y, topPanel.Width, y);
            };

            // ── Group 1: Deg / Rad ── 별도 Panel로 감싸서 그룹 분리
            rbDeg = MakeRadio("Deg", true,   4);
            rbRad = MakeRadio("Rad", false, 50);
            var grpAngle = new Panel
            {
                Location  = new Point(8, 2),
                Size      = new Size(100, 30),
                BackColor = Color.Transparent
            };
            grpAngle.Controls.Add(rbDeg);
            grpAngle.Controls.Add(rbRad);

            var sep1 = MakeSep(110);

            // ── Group 2: Dec / Hex / Bin ── 별도 Panel로 감싸서 그룹 분리
            rbDec = MakeRadio("Dec", true,   4);
            rbHex = MakeRadio("Hex", false, 54);
            rbBin = MakeRadio("Bin", false, 104);
            var grpBase = new Panel
            {
                Location  = new Point(116, 2),
                Size      = new Size(160, 30),
                BackColor = Color.Transparent
            };
            grpBase.Controls.Add(rbDec);
            grpBase.Controls.Add(rbHex);
            grpBase.Controls.Add(rbBin);

             var sep2 = MakeSep(0);
            cbEcho = new CheckBox
            {
                Text      = "Echo",
                Checked   = true,
                AutoSize  = true,
                Location  = new Point(278, 14),
                ForeColor = clrRadioFg,
                Font      = _uiFont,
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand
            };

            btnFont  = MakeBtn("🔤 Font",-318);
            btnTheme = MakeBtn("⚙ Theme", -240);
            btnVar   = MakeBtn("Var",     -162);
            btnHelp  = MakeBtn("Help",    -84);

            topPanel.Controls.AddRange(new Control[]
                { grpAngle, sep1, grpBase, sep2, cbEcho,
                  btnFont, btnTheme, btnVar, btnHelp });

            // ── Editor ────────────────────────────────────────
            rtbMain = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                Font        = _editorFont,
                BackColor   = clrEditor,
                ForeColor   = clrEditorFg,
                BorderStyle = BorderStyle.None,
                AcceptsTab  = false,
                WordWrap    = false,
                ScrollBars  = RichTextBoxScrollBars.Both,
                DetectUrls  = false
            };

            // ── Status bar ────────────────────────────────────
            statusBar = new StatusStrip { BackColor = clrStatusBg, SizingGrip = false };
            lblStatus = new ToolStripStatusLabel
            {
                ForeColor = clrStatusFg,
                Font      = new Font(_uiFont.FontFamily, 8f)
            };
            statusBar.Items.Add(lblStatus);

            // ── Assemble ──────────────────────────────────────
            this.Controls.Add(rtbMain);
            this.Controls.Add(topPanel);
            this.Controls.Add(statusBar);
            statusBar.SendToBack();
            rtbMain.BringToFront();
            // Need to dock properly
            // statusBar is docked Bottom by default in StatusStrip

            // ── Events ────────────────────────────────────────
            rtbMain.KeyDown   += RtbMain_KeyDown;
            this.KeyDown      += Form_KeyDown;
            btnHelp.Click     += (s, e) => new HelpForm(_uiFont).ShowDialog(this);
            btnVar.Click      += BtnVar_Click;
            btnFont.Click     += BtnFont_Click;
            btnTheme.Click    += BtnTheme_Click;
        }

        private void ApplyThemeToControls()
        {
            if (rtbMain == null) return;
            this.BackColor      = clrBackground;
            topPanel.BackColor  = clrTopPanel;
            rtbMain.BackColor   = clrEditor;
            rtbMain.ForeColor   = clrEditorFg;
            statusBar.BackColor = clrStatusBg;
            lblStatus.ForeColor = clrStatusFg;
            cbEcho.ForeColor    = clrRadioFg;
            foreach (var rb in new[] { rbDeg, rbRad, rbDec, rbHex, rbBin })
                rb.ForeColor = clrRadioFg;
            foreach (var btn in new[] { btnFont, btnTheme, btnVar, btnHelp })
            {
                btn.NormalColor = clrAccent;
                btn.HoverColor  = clrAccentHov;
                btn.Invalidate();
            }
            topPanel.Invalidate();
        }

        // ══════════════════════════════════════════════════════
        //  Factories
        // ══════════════════════════════════════════════════════
        private RadioButton MakeRadio(string text, bool chk, int x) => new RadioButton
        {
            Text      = text, Checked   = chk, AutoSize  = true,
            Location  = new Point(x, 13), ForeColor = clrRadioFg,
            Font      = _uiFont, BackColor = Color.Transparent, FlatStyle = FlatStyle.Flat
        };

        private Label MakeSep(int x) => new Label
        {
            Text = "", BorderStyle = BorderStyle.Fixed3D,
            Size = new Size(2, 26), Location = new Point(x, 10), BackColor = Color.Transparent
        };

        private ModernButton MakeBtn(string text, int xOffset)
        {
            var btn = new ModernButton
            {
                Text        = text,
                Size        = new Size(72, 28),
                Anchor      = AnchorStyles.Top | AnchorStyles.Right,
                NormalColor = clrAccent,
                HoverColor  = clrAccentHov,
                //Font        = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                // 이모지 지원 폰트로 변경.
                Font = new Font("Segoe UI Emoji", 8.5f),
                ForeColor   = Color.White
            };
            void SetPos() => btn.Location = new Point(topPanel.Width + xOffset - 4, 9);
            SetPos();
            topPanel.Resize += (s, e) => SetPos();
            return btn;
        }

        // ══════════════════════════════════════════════════════
        //  Font dialog
        // ══════════════════════════════════════════════════════
        private void BtnFont_Click(object sender, EventArgs e)
        {
            using var dlg = new FontPickerForm(_uiFont, _editorFont);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _uiFont     = dlg.SelectedUIFont;
                _editorFont = dlg.SelectedEditorFont;
                ApplyFonts();
                _settings.SetFont(_uiFont, _editorFont);
                _settings.Save();
            }
        }

        private void ApplyFonts()
        {
            this.Font    = _uiFont;
            rtbMain.Font = _editorFont;
            foreach (var c in new Control[] { rbDeg, rbRad, rbDec, rbHex, rbBin, cbEcho })
                c.Font = _uiFont;
        }

        // ══════════════════════════════════════════════════════
        //  Theme dialog
        // ══════════════════════════════════════════════════════
        private void BtnTheme_Click(object sender, EventArgs e)
        {
            using var dlg = new ThemePickerForm(_theme, _uiFont);
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                _theme = dlg.SelectedTheme;
                SetThemeColors();
                ApplyThemeToControls();
                _settings.SetTheme(_theme);
                _settings.Save();
            }
        }

        // ══════════════════════════════════════════════════════
        //  Key handling
        // ══════════════════════════════════════════════════════
        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if      (e.KeyCode == Keys.F1)     { new HelpForm(_uiFont).ShowDialog(this); e.Handled = true; }
            else if (e.KeyCode == Keys.F12)    { ProcessCurrentLine(); e.Handled = true; }
            else if (e.KeyCode == Keys.Insert) { InsertLastResult();   e.Handled = true; }
        }

        private void RtbMain_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+V 붙여넣기 감지
            if (e.Control && e.KeyCode == Keys.V)
            {
                e.Handled = e.SuppressKeyPress = true;
                string text = Clipboard.GetText();
                string[] lines = text.Split('\n');
                foreach (var line in lines)
                {
                    string l = line.TrimEnd('\r');
                    if (string.IsNullOrWhiteSpace(l)) continue;
                    rtbMain.AppendText(l);
                    ProcessCurrentLine();
                }
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.F12)
            {
                e.Handled = e.SuppressKeyPress = true;
                ProcessCurrentLine();
            }
        }

        private void InsertLastResult()
        {
            if (string.IsNullOrEmpty(lastResult)) return;
            int pos = rtbMain.SelectionStart;
            rtbMain.Text = rtbMain.Text.Insert(pos, lastResult);
            rtbMain.SelectionStart = pos + lastResult.Length;
        }

        // ══════════════════════════════════════════════════════
        //  Calculation
        // ══════════════════════════════════════════════════════
        private void ProcessCurrentLine()
        {
            int lineIndex = rtbMain.GetLineFromCharIndex(rtbMain.SelectionStart);
            string line   = lineIndex < rtbMain.Lines.Length ? rtbMain.Lines[lineIndex] : "";
            int hashIdx   = line.IndexOf('#');
            if (hashIdx >= 0) line = line.Substring(0, hashIdx);
            line = line.Trim();

            int lineStart = rtbMain.GetFirstCharIndexFromLine(lineIndex);
            int lineEnd   = lineStart + (lineIndex < rtbMain.Lines.Length ? rtbMain.Lines[lineIndex].Length : 0);
            rtbMain.SelectionStart = lineEnd; rtbMain.SelectionLength = 0;

            if (string.IsNullOrWhiteSpace(line)) { AppendLine(""); return; }

            string lower = line.ToLower();
            if (lower == "clr" || lower == "cls")  { rtbMain.Clear(); SetStatus("Cleared"); return; }

            if (lower.StartsWith("precision "))
            {
                if (int.TryParse(lower.Substring(10).Trim(), out int p) && p > 0 && p <= 15)
                { outputPrecision = p; AppendLine($"Precision = {p}"); }
                else AppendLine("Error: invalid precision");
                return;
            }

            if (lower.StartsWith("fact "))
            {
                try { long n = (long)Eval(line.Substring(5).Trim()); AppendLine($"fact({n}) = {Factorize(n)}"); }
                catch (Exception ex) { AppendLine($"Error: {ex.Message}"); }
                return;
            }

            if (lower.StartsWith("fx") || lower == "fxclr")
            { AppendLine("Graph: not supported in text mode"); return; }

            try
            {
                string result = EvalLine(line);
                lastResult = result;
                AppendLine(cbEcho.Checked ? $"{line} = {result}" : result);
                SetStatus($"= {result}");
            }
            catch (Exception ex)
            {
                AppendLine($"Error: {ex.Message}");
                SetStatus($"Error: {ex.Message}");
            }
        }

        private void AppendLine(string text)
        {
            rtbMain.AppendText("\n" + text + "\n");
            rtbMain.SelectionStart = rtbMain.Text.Length;
            rtbMain.ScrollToCaret();
        }

        private void SetStatus(string msg) => lblStatus.Text = "  " + msg;

        private string EvalLine(string line)
        {
            int eqIdx = line.IndexOf('=');
            if (eqIdx > 0)
            {
                string vn = line.Substring(0, eqIdx).Trim();
                if (IsIdent(vn))
                {
                    string expr = line.Substring(eqIdx + 1).Trim();
                    if (expr.Contains("i"))
                    {
                        try { var cv = EvalC(expr); complexVars[vn] = cv; return $"{vn} = {FmtC(cv)}"; }
                        catch { }
                    }
                    double v = Eval(expr);
                    variables[vn] = v;
                    return Fmt(v);
                }
            }
            if (line.Contains("i"))
            {
                try { var cv = EvalC(line); if (cv.Imaginary != 0) return FmtC(cv); }
                catch { }
            }
            return Fmt(Eval(line));
        }

        private string Fmt(double v)
        {
            if (rbHex.Checked) return $"0x{(long)v:X}";
            if (rbBin.Checked) return $"0b{Convert.ToString((long)v, 2)}";
            return v.ToString("G" + outputPrecision);
        }

        private string FmtC(System.Numerics.Complex c)
        {
            if (c.Imaginary == 0) return Fmt(c.Real);
            if (c.Real == 0) return $"{Fmt(c.Imaginary)}i";
            return $"{Fmt(c.Real)}{(c.Imaginary >= 0 ? " + " : " - ")}{Fmt(Math.Abs(c.Imaginary))}i";
        }

        private bool IsIdent(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            if (!char.IsLetter(s[0]) && s[0] != '_') return false;
            foreach (char c in s) if (!char.IsLetterOrDigit(c) && c != '_') return false;
            return true;
        }

        private string Factorize(long n)
        {
            if (n <= 1) return n.ToString();
            var sb = new StringBuilder();
            for (long i = 2; i * i <= n; i++)
                while (n % i == 0) { if (sb.Length > 0) sb.Append(" × "); sb.Append(i); n /= i; }
            if (n > 1) { if (sb.Length > 0) sb.Append(" × "); sb.Append(n); }
            return sb.ToString();
        }

        private System.Numerics.Complex EvalC(string e) =>
            new ComplexParser(e, variables, complexVars, rbDeg.Checked).Parse();

        private double Eval(string e) =>
            new ExpressionParser(e, variables, complexVars, rbDeg.Checked).Parse();

        private void BtnVar_Click(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("── Variables ──");
            foreach (var kv in variables) sb.AppendLine($"  {kv.Key} = {kv.Value}");
            foreach (var kv in complexVars) sb.AppendLine($"  {kv.Key} = {FmtC(kv.Value)}");
            MessageBox.Show(sb.ToString(), "Variables", MessageBoxButtons.OK, MessageBoxIcon.None);
        }
    }

    // ══════════════════════════════════════════════════════════
    //  ModernButton
    // ══════════════════════════════════════════════════════════
    public class ModernButton : Control
    {
        public Color NormalColor { get; set; } = Color.FromArgb(90, 75, 200);
        public Color HoverColor  { get; set; } = Color.FromArgb(120, 100, 230);
        private bool _hover, _pressed;

        public ModernButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            ForeColor = Color.White;
            Cursor    = Cursors.Hand;
        }

        protected override void OnMouseEnter(EventArgs e) { _hover   = true;  Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover   = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _pressed = true;  Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e)   { _pressed = false; Invalidate(); base.OnMouseUp(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g  = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rc  = new Rectangle(0, 0, Width - 1, Height - 1);
            var col = _pressed ? ControlPaint.Dark(NormalColor, 0.1f) : (_hover ? HoverColor : NormalColor);
            using (var path = RR(rc, 6))
            using (var b = new SolidBrush(col))
                g.FillPath(b, path);
            // Highlight
            using (var path = RR(new Rectangle(rc.X + 1, rc.Y + 1, rc.Width - 2, rc.Height / 2 - 1), 5))
            using (var b = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
                g.FillPath(b, path);
            // Text
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            using var fb = new SolidBrush(ForeColor);
            g.DrawString(Text, Font, fb, rc, sf);
        }

        private static GraphicsPath RR(Rectangle r, int rad)
        {
            var p = new GraphicsPath(); int d = rad * 2;
            p.AddArc(r.X,         r.Y,          d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y,          d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0,   90);
            p.AddArc(r.X,         r.Bottom - d, d, d, 90,  90);
            p.CloseFigure(); return p;
        }
    }

    // ══════════════════════════════════════════════════════════
    //  AppTheme enum
    // ══════════════════════════════════════════════════════════
    public enum AppTheme { Dark, Light, Monokai }
}
