using System;
using System.Collections.Generic;
using System.Numerics;

namespace Calculator
{
    /// <summary>
    /// Complex-aware expression parser.
    /// Treats standalone 'i' as the imaginary unit.
    /// </summary>
    public class ComplexParser
    {
        private string _src;
        private int _pos;
        private Dictionary<string, double> _vars;
        private Dictionary<string, Complex> _cvars;
        private bool _degrees;

        public ComplexParser(string src,
            Dictionary<string, double> vars,
            Dictionary<string, Complex> cvars,
            bool degrees)
        {
            _src = src.Trim();
            _pos = 0;
            _vars = vars;
            _cvars = cvars;
            _degrees = degrees;
        }

        public Complex Parse()
        {
            var result = ParseExpr();
            SkipWS();
            if (_pos < _src.Length)
                throw new Exception($"Unexpected character '{_src[_pos]}'");
            return result;
        }

        private void SkipWS()
        {
            while (_pos < _src.Length && _src[_pos] == ' ') _pos++;
        }

        private Complex ParseExpr()
        {
            var left = ParseTerm();
            SkipWS();
            while (_pos < _src.Length && (_src[_pos] == '+' || _src[_pos] == '-'))
            {
                char op = _src[_pos++];
                var right = ParseTerm();
                left = op == '+' ? left + right : left - right;
                SkipWS();
            }
            return left;
        }

        private Complex ParseTerm()
        {
            var left = ParsePower();
            SkipWS();
            while (_pos < _src.Length && (_src[_pos] == '*' || _src[_pos] == '/' || _src[_pos] == '%'))
            {
                char op = _src[_pos++];
                var right = ParsePower();
                if (op == '*') left *= right;
                else if (op == '/') left /= right;
                else left = new Complex(left.Real % right.Real, 0);
                SkipWS();
            }
            return left;
        }

        private Complex ParsePower()
        {
            var bas = ParseUnary();
            SkipWS();
            if (_pos < _src.Length && _src[_pos] == '^')
            {
                _pos++;
                var exp = ParseUnary();
                return Complex.Pow(bas, exp);
            }
            return bas;
        }

        private Complex ParseUnary()
        {
            SkipWS();
            if (_pos < _src.Length && _src[_pos] == '-') { _pos++; return -ParseUnary(); }
            if (_pos < _src.Length && _src[_pos] == '+') { _pos++; return ParseUnary(); }
            return ParsePrimary();
        }

        private Complex ParsePrimary()
        {
            SkipWS();
            if (_pos >= _src.Length) throw new Exception("Unexpected end");

            if (_src[_pos] == '(')
            {
                _pos++;
                var v = ParseExpr();
                SkipWS();
                if (_pos < _src.Length && _src[_pos] == ')') _pos++;
                return v;
            }

            // Number
            if (char.IsDigit(_src[_pos]) || (_src[_pos] == '.' && _pos + 1 < _src.Length && char.IsDigit(_src[_pos + 1])))
            {
                double re = ParseNumber();
                SkipWS();
                // If followed by 'i', it's an imaginary literal
                if (_pos < _src.Length && _src[_pos] == 'i' &&
                    (_pos + 1 >= _src.Length || !char.IsLetterOrDigit(_src[_pos + 1])))
                {
                    _pos++;
                    return new Complex(0, re);
                }
                return new Complex(re, 0);
            }

            // Identifier or function
            if (char.IsLetter(_src[_pos]) || _src[_pos] == '_')
            {
                int start = _pos;
                while (_pos < _src.Length && (char.IsLetterOrDigit(_src[_pos]) || _src[_pos] == '_'))
                    _pos++;
                string name = _src.Substring(start, _pos - start);
                SkipWS();

                // Standalone 'i'
                if (name == "i") return new Complex(0, 1);

                // Function
                if (_pos < _src.Length && _src[_pos] == '(')
                {
                    _pos++;
                    var args = ParseArgList();
                    SkipWS();
                    if (_pos < _src.Length && _src[_pos] == ')') _pos++;
                    return CallFunction(name.ToLower(), args);
                }

                // Variable
                if (_vars.TryGetValue(name, out double rv)) return new Complex(rv, 0);
                if (_cvars.TryGetValue(name, out Complex cv)) return cv;
                throw new Exception($"Unknown variable: {name}");
            }

            throw new Exception($"Unexpected '{_src[_pos]}'");
        }

        private double ParseNumber()
        {
            int start = _pos;
            while (_pos < _src.Length && (char.IsDigit(_src[_pos]) || _src[_pos] == '.')) _pos++;
            if (_pos < _src.Length && (_src[_pos] == 'e' || _src[_pos] == 'E'))
            {
                _pos++;
                if (_pos < _src.Length && (_src[_pos] == '+' || _src[_pos] == '-')) _pos++;
                while (_pos < _src.Length && char.IsDigit(_src[_pos])) _pos++;
            }
            return double.Parse(_src.Substring(start, _pos - start),
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private List<Complex> ParseArgList()
        {
            var args = new List<Complex>();
            SkipWS();
            if (_pos < _src.Length && _src[_pos] == ')') return args;
            args.Add(ParseExpr());
            SkipWS();
            while (_pos < _src.Length && _src[_pos] == ',')
            {
                _pos++;
                args.Add(ParseExpr());
                SkipWS();
            }
            return args;
        }

        private double Deg2Rad(double x) => _degrees ? x * Math.PI / 180.0 : x;
        private double Rad2Deg(double x) => _degrees ? x * 180.0 / Math.PI : x;

        private Complex CallFunction(string name, List<Complex> args)
        {
            var a = args.Count > 0 ? args[0] : Complex.Zero;
            var b = args.Count > 1 ? args[1] : Complex.Zero;

            switch (name)
            {
                case "sin":  return Complex.Sin(new Complex(Deg2Rad(a.Real), a.Imaginary));
                case "cos":  return Complex.Cos(new Complex(Deg2Rad(a.Real), a.Imaginary));
                case "tan":  return Complex.Tan(new Complex(Deg2Rad(a.Real), a.Imaginary));
                case "sinh": return Complex.Sinh(a);
                case "cosh": return Complex.Cosh(a);
                case "tanh": return Complex.Tanh(a);
                case "asin": return Complex.Asin(a);
                case "acos": return Complex.Acos(a);
                case "atan": return Complex.Atan(a);
                case "ln":   return Complex.Log(a);
                case "log":  return Complex.Log10(a);
                case "exp":  return Complex.Exp(a);
                case "pow":  return Complex.Pow(a, b);
                case "sqr":
                case "sqrt": return Complex.Sqrt(a);
                case "abs":  return new Complex(Complex.Abs(a), 0);
                case "re":   return new Complex(a.Real, 0);
                case "im":   return new Complex(a.Imaginary, 0);
                case "norm": return new Complex(a.Magnitude * a.Magnitude, 0);
                case "pol":  return Complex.FromPolarCoordinates(a.Real, Deg2Rad(b.Real));
                default: throw new Exception($"Unknown function: {name}");
            }
        }
    }
}
