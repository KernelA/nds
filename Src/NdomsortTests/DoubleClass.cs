namespace NdomsortTests
{
    using System;

    /// <summary>
    /// For the tests only. 
    /// </summary>
    internal class DoubleClass : IComparable<DoubleClass>, IEquatable<DoubleClass>
    {
        private double _value;

        public double Value => _value;

        public DoubleClass(double Value)
        {
            _value = Value;
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