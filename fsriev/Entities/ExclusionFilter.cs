using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TehGM.Fsriev
{
    class ExclusionFilter : IEquatable<ExclusionFilter>
    {
        public string Pattern { get; }
        public Regex Regex { get; }

        public ExclusionFilter(string pattern, Regex regex)
        {
            this.Pattern = pattern;
            this.Regex = regex;
        }

        public static ExclusionFilter Build(string pattern)
        {
            string rgx = $"^{Regex.Escape(pattern).Replace("\\*", ".*")}$";
            return new ExclusionFilter(pattern, new Regex(rgx, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant));
        }

        public bool Excludes(string path)
            => this.Regex.IsMatch(path);

        public override bool Equals(object obj)
            => Equals(obj as ExclusionFilter);

        public bool Equals(ExclusionFilter other)
            => other != null && Pattern == other.Pattern;

        public override int GetHashCode()
            => this.Pattern.GetHashCode();

        public override string ToString()
            => this.Pattern;

        public static bool operator ==(ExclusionFilter left, ExclusionFilter right)
            => EqualityComparer<ExclusionFilter>.Default.Equals(left, right);

        public static bool operator !=(ExclusionFilter left, ExclusionFilter right)
            => !(left == right);
    }
}
