namespace NdomsortTests
{
    using System;

    /// <summary>
    /// For the tests only. 
    /// </summary>
    internal class DoubleClass : IComparable<DoubleClass>, IEquatable<DoubleClass>
    {
        public double Value { get; private set; }

        public DoubleClass(double Value)
        {
            this.Value = Value;
        }

        public int CompareTo(DoubleClass Other)
        {
            if (Other == null)
            {
                throw new ArgumentNullException(nameof(Other));
            }

            return Value.CompareTo(Other.Value);
        }

        public bool Equals(DoubleClass Other)
        {
            if (Other == null)
            {
                return false;
            }
            else
            {
                return Value == Other.Value;
            }
        }
    }
}