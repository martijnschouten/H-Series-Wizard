using System.Globalization;
using System.Text;
using System.Collections.Generic;

namespace DiabasePrintingWizard
{
    class GCodeLine
    {
        public string Content { get; set; }

        public double Feedrate { get; set; }        // in mm/s

        public double? Distance { get; set; }

        public double? Duration { get; set; }

        public GCodeLine(string content) => Content = content;
        public GCodeLine(string content, double feedrate)
        {
            Content = content;
            Feedrate = feedrate;
        }

        public string DebugLine()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Content);
            if (Distance.HasValue || Duration.HasValue)
            {
                sb.Append("; ");
                if (Duration.HasValue)
                {
                    sb.Append($"Duration: {Duration.Value.ToString("F3", FrmMain.numberFormat)}{(Distance.HasValue ? ", " : "")}");
                }
                if (Distance.HasValue)
                {
                    sb.Append($"Distance: {Distance.Value.ToString("F3", FrmMain.numberFormat)}");
                }
            }
            return sb.ToString();
        }

        public bool UpdateFValue(char chr, double newValue)
        {
            if (FindParameter(chr, out string expression, true) && expression != "")
            {
                this.Content = this.Content.Replace($"{chr}{expression}", $"{chr}{newValue.ToString("F3", FrmMain.numberFormat)}");
                return true;
            }
            return false;
        }

        public int? GetIValue(char chr, bool allowInComment = false)
        {
            if (FindParameter(chr, out string expression, false, allowInComment) && expression != "")
            {
                return int.TryParse(expression, out int result) ? new int?(result) : null;
            }
            return null;
        }

        public double? GetFValue(char chr, bool allowInComment = false)
        {
            if (FindParameter(chr, out string expression, true, allowInComment) && expression != "")
            {
                return double.TryParse(
                        expression,
                        NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                        FrmMain.numberFormat,
                        out double result)
                    ? new double?(result) : null;
            }
            return null;
        }

        public List<double> GetArrayValue(char startChr, char seperator, bool decimalNumber)
        {
            List<double> values = new List<double>();
            bool started = false;
            string expression = "";
            foreach (char c in Content)
            {
                if (c == startChr)
                {
                    started = true;
                }
                if (started)
                {
                    if (c == seperator)
                    {
                        double.TryParse(expression, out double result);
                        values.Add(result);
                        expression = "";
                    }
                    else if(c == '-'||char.IsDigit(c)|| (decimalNumber && c == '.'))
                    {
                        expression += c;
                    }
                }
            }
            if(started && (expression != ""))
            {
                double.TryParse(expression, out double result);
                values.Add(result);
            }
            return values;
        }


        private bool FindParameter(char chr, out string expression, bool decimalNumber, bool allowCommentLine = false)
        {
            expression = "";
            bool inComment = false;
            bool found = false;
            foreach (char c in Content)
            {
                if (c == ';' && !allowCommentLine)
                {
                    break;
                }

                if (c == '(')
                {
                    inComment = true;
                }
                else if (inComment)
                {
                    inComment = c != ')';
                }
                else if (c == chr)
                {
                    found = true;
                }
                else if (found && !char.IsWhiteSpace(c))
                {
                    if (c == '-' || char.IsDigit(c) || (decimalNumber && c == '.'))
                    {
                        expression += c;
                    }
                    else if (c != '+')
                    {
                        break;
                    }
                }
            }
            return found;
        }
    }
}