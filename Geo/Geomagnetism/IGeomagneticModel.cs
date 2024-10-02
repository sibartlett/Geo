using System;

namespace Geo.Geomagnetism;

public interface IGeomagneticModel
{
    DateTime ValidFrom { get; }
    DateTime ValidTo { get; }

    /// <summary>
    ///     G Gauss coefficients of main geomagnetic model (nT)
    /// </summary>
    double[,] MainCoefficientsG { get; }

    /// <summary>
    ///     H Gauss coefficients of main geomagnetic model (nT)
    /// </summary>
    double[,] MainCoefficientsH { get; }

    /// <summary>
    ///     G Gauss coefficients of secular geomagnetic model (nT/yr)
    /// </summary>
    double[,] SecularCoefficientsG { get; }

    /// <summary>
    ///     H Gauss coefficients of secular geomagnetic model (nT/yr)
    /// </summary>
    double[,] SecularCoefficientsH { get; }
}
