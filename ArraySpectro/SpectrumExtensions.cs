using At.Matus.OpticalSpectrumLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArraySpectro
{
    internal static class SpectrumExtensions
    {
        internal static string ToCsvLines(this IOpticalSpectrum spectrum)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < spectrum.NumberOfPoints; i++)
            {
                sb.AppendLine(spectrum.DataPoints[i].ToCsvLine());
            }
            return sb.ToString();
        }

        internal static void WriteToCsvFile(this IOpticalSpectrum spectrum, string filePath)
        {
            string csvString = spectrum.ToCsvLines();
            File.WriteAllText(filePath, csvString);
        }

    }
}
