using At.Matus.OpticalSpectrumLib;
using System;

namespace ArraySpectro
{
    internal partial class Program
    {
        internal static void InvestigateDarkCurrent(double intTime, int nSamples)
        {
            MeasuredOpticalSpectrum signal = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            MeasuredOpticalSpectrum dark = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            spectro.SetIntegrationTime(intTime);

            Console.WriteLine($"Integration time: {spectro.GetIntegrationTime()} s");
            Console.WriteLine($"Instrument Temperature: {spectro.Temperature} °C");

            spectro.OpenShutter();
            OnCallUpdateSpectrum(signal, nSamples, "Signal Spectrum #1");
            spectro.CloseShutter();
            OnCallUpdateSpectrum(dark, nSamples, "Dark Spectrum #1");

            OpticalSpectrum correctedAB = SpecMath.Subtract(signal, dark);

            OnCallUpdateSpectrum(dark, nSamples, "Dark Spectrum #2");
            spectro.OpenShutter();
            OnCallUpdateSpectrum(signal, nSamples, "Signal Spectrum #2");

            OpticalSpectrum correctedABBA = SpecMath.Subtract(signal, dark);

            string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            Console.WriteLine($"Average signal {correctedABBA.GetSignalStatistics().AverageValue:F3} +/- {correctedABBA.GetSignalStatistics().StandardDeviation:F3}");
            logFile.WriteLine($"{timeStamp}   {spectro.Temperature:F2} °C   ABBA: {correctedABBA.GetSignalStatistics().AverageValue,6:F3} +/- {correctedABBA.GetSignalStatistics().StandardDeviation:F3}   AB: {correctedAB.GetSignalStatistics().AverageValue,6:F3} +/- {correctedAB.GetSignalStatistics().StandardDeviation:F3}");
            logFile.Flush();

            string fileName = $"S-D_t{(int)(intTime*1000)}_n{nSamples}_{timeStamp}.csv";
            correctedABBA.WriteToCsvFile(fileName);

            Console.WriteLine();
        }
    }
}
