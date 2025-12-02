using At.Matus.OpticalSpectrumLib;
using At.Matus.StatisticPod;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArraySpectro
{
    public static class SpectrumExtensions
    {
        public static string ToMetaDataString(this IOpticalSpectrum spectrum)
        {
            Dictionary<string, string> dict = spectrum.MetaData.Records;
            StringBuilder sb = new StringBuilder();
            foreach (var key in dict.Keys)
            {
                sb.AppendLine($"{key} = {dict[key]}");
            }
            return sb.ToString();
        }

        public static string ToCsvString(this IOpticalSpectrum spectrum)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(spectrum.DataPoints[0].GetCsvHeader());
            for (int i = 0; i < spectrum.NumberOfPoints; i++)
            {
                sb.AppendLine(spectrum.DataPoints[i].ToCsvLine());
            }
            return sb.ToString();
        }

        public static void WriteToCsvFile(this IOpticalSpectrum spectrum, string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(spectrum.ToMetaDataString());
            sb.Append(spectrum.ToCsvString());
            File.WriteAllText(filePath, sb.ToString());
        }

        public static StatisticPod GetSignalStatistics(this IOpticalSpectrum spectrum)
        {
            StatisticPod stats = new StatisticPod();
            for (int i = 0; i < spectrum.NumberOfPoints; i++)
            {
                stats.Update(spectrum.DataPoints[i].Signal);
            }
            return stats;
        }

    }
}
