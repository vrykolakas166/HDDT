using System;

namespace HDDT.App
{
    public class Invoice : IEquatable<Invoice>
    {
        public string Symbol { get; set; }
        public string Number { get; set; }
        public string Date { get; set; }
        public string Information { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Invoice);
        }

        public bool Equals(Invoice other)
        {
            if (other == null) return false;

            return string.Equals(Symbol, other.Symbol) &&
                   string.Equals(Number, other.Number) &&
                   string.Equals(Date, other.Date) &&
                   string.Equals(Information, other.Information);
        }

        public override int GetHashCode()
        {
            // Combine hash codes of the properties
            int hash = 17;
            hash = hash * 23 + (Symbol?.GetHashCode() ?? 0);
            hash = hash * 23 + (Number?.GetHashCode() ?? 0);
            hash = hash * 23 + (Date?.GetHashCode() ?? 0);
            hash = hash * 23 + (Information?.GetHashCode() ?? 0);
            return hash;
        }

        public static bool operator ==(Invoice left, Invoice right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }

        public static bool operator !=(Invoice left, Invoice right)
        {
            return !(left == right);
        }
    }
}