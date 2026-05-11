using System;
using System.Drawing;
using System.Windows.Forms;

namespace Calculator
{
    public class HelpForm : Form
    {
        public HelpForm(Font uiFont = null)
        {
            if (uiFont != null) this.Font = uiFont;
            this.Text = "Calculator Help !!!";
            this.Size = new Size(600, 700);
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            var header = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = SystemColors.Control };

            var lblTitle = new Label
            {
                Text = "Calculator v1.0",
                Font = new Font("Courier New", 11f, FontStyle.Bold),
                Location = new Point(10, 18),
                AutoSize = true
            };

            var btnClose = new Button
            {
                Text = "Close!",
                Size = new Size(70, 28),
                Location = new Point(506, 16),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.System
            };
            btnClose.Click += (s, e) => this.Close();

            header.Controls.Add(lblTitle);
            header.Controls.Add(btnClose);

            var tb = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Courier New", 9f),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.Vertical,
                Text = GetHelpText()
            };

            this.Controls.Add(tb);
            this.Controls.Add(header);
        }

        private string GetHelpText()
        {
            return
@"Write in the line that you want to calculate and
press enter to get the result. The calculator can
handle variables and complex numbers. An 'i' after
the number indicates an imaginary number. An '#'
comments the rest of a line. Press F1 to open help.

Functions:

  sin (x)      - Sine of x
  cos (x)      - Cosine of x
  tan (x)      - Tangent of x
  sinh(x)      - Hyperbolic sine of x
  cosh(x)      - Hyperbolic cosine of x
  tanh(x)      - Hyperbolic tangent of x
  asin(x)      - Arcsine of x
  acos(x)      - Arccosine of x
  atan(x)      - Arctangent of x
  atan(x,y)    - Arctangent of x/y (atan2)

  ln  (x)      - Logarithm of x. Base e
  log (x)      - Logarithm of x. Base 10
  exp (x)      - e raised to the power of x
  pow (x , y)  - x raised to the power y
  sqr (x)      - Square-root of x
  sqrn(x , y)  - y-th root of x: sqrn(8,3) = 2

  !   (x)      - Factorial of x
  rnd (x)      - Random value between 0 and x
  abs (x)      - Absolute value of x
  int (x)      - The integer value of x
  max (x , y)  - Maximum of x and y
  min (x , y)  - Minimum of x and y

  re  (x)      - Real part of x
  im  (x)      - Imaginary part of x
  norm(x)      - Squared magnitude of x
  pol (x , y)  - Complex value: magnitude x, phase y

Constants:

  pi = 3.1415...
  e  = 2.7182...

Keys and keywords:

  F1     - Open this help
  F12    - Calculate current line
  Insert - Inserts the last result

  clr, cls -> Clears the screen
  precision nr -> Sets output precision to nr

  fact x -> factoring x into a product of primes

Examples:

  100+(2+3)*(4+5)   -> Result: 145
  100%6             -> Result: 4  (Remainder)
  pow(10/5,3)       -> Result: 8
  a=10              -> Result: 10, and a=10
  bc = sqr(16)      -> Result: 4,  and bc=4
  a + bc            -> Result: 14
  0b10011           -> Result: 19  (Bin -> Dec)
  0xFF              -> Result: 255 (Hex -> Dec)
  2 * (5 + 4i)      -> Complex result: 10 + 8i
  a = 5 + 7i        -> Complex result: a = 5 + 7i
  fact 12           -> 2 * 2 * 3

Revision by 
ver 1.0 2026.05.11
";
        }
    }
}
