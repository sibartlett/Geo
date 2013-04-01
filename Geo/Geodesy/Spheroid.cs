using System;

namespace Geo.Geodesy
{
    public class Spheroid
    {
        public static readonly Spheroid Default = Wgs84;

        public static Spheroid Wgs84
        {
            get
            {
                return new Spheroid("WGS84", 6378137d, 298.257223563d);
            }
        }

        public static Spheroid Grs80
        {
            get
            {
                return new Spheroid("GRS80", 6378137d, 298.257222101);
            }
        }

        public static Spheroid International1924
        {
            get
            {
                return new Spheroid("International 1924", 6378388d, 297d);
            }
        }

        public static Spheroid Clarke1866
        {
            get
            {
                return new Spheroid("Clarke 1866", 6378206.4, 294.9786982);
            }
        }

        public Spheroid(string name, double equatorialAxis, double inverseFlattening)
        {
            Name = name;
            InverseFlattening = inverseFlattening;
            Flattening = 1 / inverseFlattening;
            EquatorialAxis = equatorialAxis;
            PolarAxis = equatorialAxis * (1 - 1 / inverseFlattening);
            Eccentricity = Math.Sqrt(2 * Flattening - Flattening * Flattening);
            MeanRadius = (2*EquatorialAxis + PolarAxis)/3;
            IsSphere = Math.Abs(EquatorialAxis - PolarAxis) < double.Epsilon;
        }

        public string Name { get; private set; }
        public double Flattening { get; private set; }
        public double InverseFlattening { get; private set; }
        public double EquatorialAxis { get; private set; }
        public double PolarAxis { get; private set; }
        public double Eccentricity { get; private set; }
        public double MeanRadius { get; private set; }
        public bool IsSphere { get; private set; }
    }
}
