using System;
using System.Collections.Generic;
using System.Text;

namespace Calculator
{
    /// <summary>
    /// Recursive-descent parser for real-valued expressions.
    /// Supports: +, -, *, /, %, ^, (, )
    /// Functions: sin, cos, tan, sinh, cosh, tanh, asin, acos, atan,
    ///            ln, log, exp, sqr, sqrt, pow, sqrn, !, rnd, abs, int,
    ///            max, min, cpx, re, im, norm, pol, fact
    /// Variables and constants.
    /// Hex (0x...) and Binary (0b...) literals.
    /// </summary>
    public class ExpressionParser
    {
        protected string _src;
        protected int _pos;
        protected Dictionary<string, double> _vars;
        protected Dictionary<string, System.Numerics.Complex> _cvars;
        protected bool _degrees;

        public ExpressionParser(string src, Dictionary<string, double> vars,
            Dictionary<string, System.Numerics.Complex> cvars, bool degrees)
        {
            _src = src.Trim();
            _pos = 0;
            _vars = vars;
            _cvars = cvars;
            _degrees = degrees;
        }

        public double Parse()
        {
            double result = ParseExpr();
            SkipWS();
            if (_pos < _src.Length)
                throw new Exception($"Unexpected character '{_src[_pos]}'");
            return result;
        }

        protected void SkipWS()
        {
            while (_pos < _src.Length && _src[_pos] == ' ')
                _pos++;
        }

        protected double ParseExpr()
        {
            double left = ParseTerm();
            SkipWS();
            while (_pos < _src.Length && (_src[_pos] == '+' || _src[_pos] == '-'))
            {
                char op = _src[_pos++];
                double right = ParseTerm();
                left = op == '+' ? left + right : left - right;
                SkipWS();
            }
            return left;
        }

        protected double ParseTerm()
        {
            double left = ParsePower();
            SkipWS();
            while (_pos < _src.Length && (_src[_pos] == '*' || _src[_pos] == '/' || _src[_pos] == '%'))
            {
                char op = _src[_pos++];
                double right = ParsePower();
                if (op == '*') left *= right;
                else if (op == '/') left /= right;
                else left = left % right;
                SkipWS();
            }
            return left;
        }

        protected double ParsePower()
        {
            double bas = ParseUnary();
            SkipWS();
            if (_pos < _src.Length && _src[_pos] == '^')
            {
                _pos++;
                double exp = ParseUnary();
                return Math.Pow(bas, exp);
            }
            return bas;
        }

        protected double ParseUnary()
        {
            SkipWS();
            if (_pos < _src.Length && _src[_pos] == '-')
            {
                _pos++;
                return -ParseUnary();
            }
            if (_pos < _src.Length && _src[_pos] == '+')
            {
                _pos++;
                return ParseUnary();
            }
            return ParsePrimary();
        }

        protected virtual double ParsePrimary()
        {
            SkipWS();
            if (_pos >= _src.Length)
                throw new Exception("Unexpected end of expression");

            // Parenthesis
            if (_src[_pos] == '(')
            {
                _pos++;
                double val = ParseExpr();
                SkipWS();
                if (_pos < _src.Length && _src[_pos] == ')') _pos++;
                else throw new Exception("Missing closing ')'");
                return PostfixOps(val);
            }

            // Hex literal 0x...
            if (_pos + 1 < _src.Length && _src[_pos] == '0' &&
                (_src[_pos + 1] == 'x' || _src[_pos + 1] == 'X'))
            {
                _pos += 2;
                var sb = new StringBuilder();
                while (_pos < _src.Length && IsHexDigit(_src[_pos]))
                    sb.Append(_src[_pos++]);
                return Convert.ToInt64(sb.ToString(), 16);
            }

            // Bin literal 0b...
            if (_pos + 1 < _src.Length && _src[_pos] == '0' &&
                (_src[_pos + 1] == 'b' || _src[_pos + 1] == 'B'))
            {
                _pos += 2;
                var sb = new StringBuilder();
                while (_pos < _src.Length && (_src[_pos] == '0' || _src[_pos] == '1'))
                    sb.Append(_src[_pos++]);
                return Convert.ToInt64(sb.ToString(), 2);
            }

            // Number
            if (char.IsDigit(_src[_pos]) || (_src[_pos] == '.' && _pos + 1 < _src.Length && char.IsDigit(_src[_pos + 1])))
            {
                return ParseNumber();
            }

            // Identifier or function
            if (char.IsLetter(_src[_pos]) || _src[_pos] == '_')
            {
                return ParseIdentifierOrFunction();
            }

            // Factorial shorthand: !(x)
            if (_src[_pos] == '!')
            {
                _pos++;
                SkipWS();
                double val;
                if (_pos < _src.Length && _src[_pos] == '(')
                {
                    _pos++;
                    val = ParseExpr();
                    SkipWS();
                    if (_pos < _src.Length && _src[_pos] == ')') _pos++;
                }
                else val = ParsePrimary();
                return Factorial((long)val);
            }

            throw new Exception($"Unexpected character '{_src[_pos]}'");
        }

        protected double PostfixOps(double val)
        {
            SkipWS();
            while (_pos < _src.Length && _src[_pos] == '!')
            {
                _pos++;
                val = Factorial((long)val);
                SkipWS();
            }
            return val;
        }

        protected double ParseNumber()
        {
            int start = _pos;
            while (_pos < _src.Length && (char.IsDigit(_src[_pos]) || _src[_pos] == '.'))
                _pos++;
            if (_pos < _src.Length && (_src[_pos] == 'e' || _src[_pos] == 'E'))
            {
                _pos++;
                if (_pos < _src.Length && (_src[_pos] == '+' || _src[_pos] == '-')) _pos++;
                while (_pos < _src.Length && char.IsDigit(_src[_pos])) _pos++;
            }
            return double.Parse(_src.Substring(start, _pos - start),
                System.Globalization.CultureInfo.InvariantCulture);
        }

        protected virtual double ParseIdentifierOrFunction()
        {
            int start = _pos;
            while (_pos < _src.Length && (char.IsLetterOrDigit(_src[_pos]) || _src[_pos] == '_'))
                _pos++;
            string name = _src.Substring(start, _pos - start);
            SkipWS();

            // Function call
            if (_pos < _src.Length && _src[_pos] == '(')
            {
                _pos++;
                var args = ParseArgList();
                SkipWS();
                if (_pos < _src.Length && _src[_pos] == ')') _pos++;
                return CallFunction(name.ToLower(), args);
            }

            // Variable
            if (_vars.TryGetValue(name, out double vval)) return vval;
            if (_cvars.TryGetValue(name, out var cv)) return cv.Real;

            throw new Exception($"Unknown variable: {name}");
        }

        protected List<double> ParseArgList()
        {
            var args = new List<double>();
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

        protected double ToRad(double x) => _degrees ? x * Math.PI / 180.0 : x;
        protected double FromRad(double x) => _degrees ? x * 180.0 / Math.PI : x;

        protected double CallFunction(string name, List<double> args)
        {
            double a = args.Count > 0 ? args[0] : 0;
            double b = args.Count > 1 ? args[1] : 0;
            switch (name)
            {
                case "sin":   return Math.Sin(ToRad(a));
                case "cos":   return Math.Cos(ToRad(a));
                case "tan":   return Math.Tan(ToRad(a));
                case "sinh":  return Math.Sinh(a);
                case "cosh":  return Math.Cosh(a);
                case "tanh":  return Math.Tanh(a);
                case "asin":  return FromRad(Math.Asin(a));
                case "acos":  return FromRad(Math.Acos(a));
                case "atan":  return args.Count >= 2 ? FromRad(Math.Atan2(a, b)) : FromRad(Math.Atan(a));
                case "ln":    return Math.Log(a);
                case "log":   return Math.Log10(a);
                case "exp":   return Math.Exp(a);
                case "pow":   return Math.Pow(a, b);
                case "sqr":
                case "sqrt":  return Math.Sqrt(a);
                case "sqrn":  return Math.Pow(a, 1.0 / b);
                case "abs":   return Math.Abs(a);
                case "int":   return Math.Truncate(a);
                case "rnd":   return new Random().NextDouble() * a;
                case "max":   return Math.Max(a, b);
                case "min":   return Math.Min(a, b);
                case "cpx":   return (a <= (args.Count > 2 ? args[2] : double.MaxValue) && a > b) ? 1 : 0;
                case "re":    { if (_cvars.TryGetValue("_last", out var cv)) return cv.Real; return a; }
                case "im":    { if (_cvars.TryGetValue("_last", out var cv)) return cv.Imaginary; return 0; }
                case "norm":  { if (_cvars.TryGetValue("_last", out var cv)) return cv.Magnitude * cv.Magnitude; return a * a; }
                case "fact":  return Factorial((long)a);
                default: throw new Exception($"Unknown function: {name}");
            }
        }

        protected double Factorial(long n)
        {
            if (n < 0) throw new Exception("Factorial of negative number");
            if (n > 20) throw new Exception("Factorial too large");
            double r = 1;
            for (long i = 2; i <= n; i++) r *= i;
            return r;
        }

        protected bool IsHexDigit(char c) =>
            (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }
}
