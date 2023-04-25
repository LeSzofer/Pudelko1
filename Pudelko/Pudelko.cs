using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MyLib
{
    public enum UnitOfMeasure
    {
        Millimeter,
        Centimeter,
        Meter
    }

    public sealed class Pudelko : IFormattable, IEquatable<Pudelko>, IEnumerable<double>
    {
        private const double DefaultEdge = 0.1;
        private const double MaxEdgeInMeters = 10.0;

        private readonly double _a;
        private readonly double _b;
        private readonly double _c;

        public double A => Math.Round(_a, 3);
        public double B => Math.Round(_b, 3);
        public double C => Math.Round(_c, 3);

        public Pudelko(double a = DefaultEdge, double b = DefaultEdge, double c = DefaultEdge, UnitOfMeasure unit = UnitOfMeasure.Meter)
        {
            double[] dimensions = { a, b, c };

            for (int i = 0; i < dimensions.Length; i++)
            {
                switch (unit)
                {
                    case UnitOfMeasure.Centimeter:
                        dimensions[i] /= 100;
                        break;
                    case UnitOfMeasure.Millimeter:
                        dimensions[i] /= 1000;
                        break;
                }

                if (dimensions[i] <= 0 || dimensions[i] > MaxEdgeInMeters)
                {
                    throw new ArgumentOutOfRangeException($"Wymiar {i + 1} jest poza dopuszczalnym zakresem.");
                }
            }

            _a = dimensions[0];
            _b = dimensions[1];
            _c = dimensions[2];
        }

        public static Pudelko operator +(Pudelko p1, Pudelko p2)
        {
            double[] dimensions = {
                Math.Max(p1.A, p2.A),
                Math.Max(p1.B, p2.B),
                Math.Max(p1.C, p2.C)
            };

            return new Pudelko(dimensions[0], dimensions[1], dimensions[2]);
        }

        public static bool operator ==(Pudelko p1, Pudelko p2) => p1.Equals(p2);
        public static bool operator !=(Pudelko p1, Pudelko p2) => !p1.Equals(p2);

        public static explicit operator double[](Pudelko p) => new[] { p.A, p.B, p.C };
        public static implicit operator Pudelko((int a, int b, int c) dimensions) => new Pudelko(dimensions.a, dimensions.b, dimensions.c, UnitOfMeasure.Millimeter);

        public double Objetosc => Math.Round(_a * _b * _c, 9);
        public double Pole => Math.Round(2 * (_a * _b + _a * _c + _b * _c), 6);

        public bool Equals(Pudelko other)
        {
            if (other == null) return false;

            double[] thisDimensions = { A, B, C };
            double[] otherDimensions = { other.A, other.B, other.C };

            Array.Sort(thisDimensions);
            Array.Sort(otherDimensions);

            return thisDimensions.SequenceEqual(otherDimensions);
        }

        public override bool Equals(object obj) => Equals(obj as Pudelko);
        public override int GetHashCode() => (A, B, C).GetHashCode();
        public override string ToString() => ToString("m", CultureInfo.CurrentCulture);
        public string ToString(string format) => ToString(format, CultureInfo.CurrentCulture);

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "m";

            double multiplier;
            string unit;

            switch (format.ToLowerInvariant())
            {
                case "m":
                    multiplier = 1;
                    unit = "m";
                    break;
                case "cm":
                    multiplier = 100;
                    unit = "cm";
                    break;
                case "mm":
                    multiplier = 1000;
                    unit = "mm";
                    break;
                default:
                    throw new FormatException($"Nieprawidłowy format: {format}");
            }

            return $"{A * multiplier} {unit} × {B * multiplier} {unit} × {C * multiplier} {unit}";
        }

        public IEnumerator<double> GetEnumerator()
        {
            yield return A;
            yield return B;
            yield return C;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static Pudelko Parse(string input)
        {
            string[] parts = input.Split(new[] { ' ', '×' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 6) throw new FormatException("Nieprawidłowy format danych wejściowych.");

            double[] dimensions = new double[3];
            UnitOfMeasure unit = UnitOfMeasure.Meter;

            for (int i = 0; i < 3; i++)
            {
                if (!double.TryParse(parts[i * 2], NumberStyles.Float, CultureInfo.InvariantCulture, out dimensions[i]))
                {
                    throw new FormatException($"Nie udało się przetworzyć liczby: {parts[i * 2]}");
                }

                switch (parts[i * 2 + 1].ToLowerInvariant())
                {
                    case "m":
                        unit = UnitOfMeasure.Meter;
                        break;
                    case "cm":
                        unit = UnitOfMeasure.Centimeter;
                        break;
                    case "mm":
                        unit = UnitOfMeasure.Millimeter;
                        break;
                    default:
                        throw new FormatException($"Nieprawidłowa jednostka miary: {parts[i * 2 + 1]}");
                }
            }

            return new Pudelko(dimensions[0], dimensions[1], dimensions[2], unit);
        }
    }
}