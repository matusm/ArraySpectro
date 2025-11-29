using At.Matus.OpticalSpectrumLib;
using At.Matus.StatisticPod;
using Bev.Instruments.Thorlabs.Ctt;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace ArraySpectro
{
    internal partial class Program
    {
        private static ThorlabsCct spectro;
        private static StreamWriter logFile;

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            spectro = new ThorlabsCct();

            Console.WriteLine($"Instrument Manufacturer:  {spectro.InstrumentManufacturer}");
            Console.WriteLine($"Instrument Type:          {spectro.InstrumentType}");
            Console.WriteLine($"Instrument Serial Number: {spectro.InstrumentSerialNumber}");
            Console.WriteLine($"Firmware Revision:        {spectro.InstrumentFirmwareVersion}");
            Console.WriteLine($"Driver Revision:          {spectro.InstrumentElectronicsId}");
            Console.WriteLine($"Min Wavelength:           {spectro.MinimumWavelength:F2} nm");
            Console.WriteLine($"Max Wavelength:           {spectro.MaximumWavelength:F2} nm");
            Console.WriteLine($"Integration Time:         {spectro.GetIntegrationTime()} s");
            Console.WriteLine($"Is Shutter Open:          {spectro.IsShutterOpen}");
            Console.WriteLine($"Temperature:              {spectro.Temperature} °C");
            Console.WriteLine($"Hardware averaging:       {spectro.GetHardwareAveraging()}");
            Console.WriteLine($"ADC resolution:           {spectro.AdcBits} bit");
            Console.WriteLine($"ADC offset:               {spectro.AdcOffset} mV");
            Console.WriteLine($"ADC gain:                 {spectro.AdcGain} dB");

            Console.WriteLine();

            //Console.WriteLine("Estimating optimal integration time...");
            //double optimalIntegrationTime = spectro.GetOptimalExposureTime();
            //Console.WriteLine($"Optimal Integration Time: {optimalIntegrationTime} s");
            //Console.WriteLine();
            //spectro.SetIntegrationTime(optimalIntegrationTime);

            double testIntTime = 0.5;
            int nSamples = 2000;
            int idleTimeMs = 200000;

            spectro.SetIntegrationTime(testIntTime);
            spectro.CloseShutter();

            logFile = new StreamWriter($"DarkCurrentDebug4_{(int)(testIntTime * 1000)}ms.txt", append: true);
            DateTime logStartTime = DateTime.Now;

            for (int j = 0; j < 50; j++)
            {
                for (int i = 0; i <= nSamples; i++)
                {
                    var spectrumA = GetSpectrum();
                    var tempA = spectro.Temperature;
                    Thread.Sleep((int)(testIntTime * 1000));
                    var spectrumB = GetSpectrum();
                    var tempB = spectro.Temperature;
                    Thread.Sleep((int)(testIntTime * 1000));
                    var spectrumAB = SpecMath.Subtract(spectrumA, spectrumB);
                    double tempAB = (tempA + tempB) / 2.0;

                    StatisticPod statsA = spectrumA.GetSignalStatistics();
                    StatisticPod statsB = spectrumB.GetSignalStatistics();
                    StatisticPod statsAB = spectrumAB.GetSignalStatistics();
                    DateTime currentTime = DateTime.Now;
                    string logLine = $"{i,6}/{j,2}  {(currentTime - logStartTime).TotalSeconds,6:F0}   {tempAB:F2} °C    A: {statsA.AverageValue,6:F3} +/- {statsA.StandardDeviation:F3}    B: {statsB.AverageValue,6:F3} +/- {statsB.StandardDeviation:F3}    A-B: {statsAB.AverageValue,6:F3} +/- {statsAB.StandardDeviation:F3}";
                    Console.WriteLine($"{logLine}");
                    logFile.WriteLine(logLine);
                    logFile.Flush();
                    //InvestigateDarkCurrent(testIntTime, nSamples);
                }
                Console.WriteLine("waiting...");
                Thread.Sleep(idleTimeMs);
                Console.WriteLine("resuming...");
            }

            Console.WriteLine();
            logFile.Close();
            Console.WriteLine("done.");

        }

        internal static MeasuredOpticalSpectrum GetSpectrum()
        {
            MeasuredOpticalSpectrum spectrum = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            spectrum.UpdateSignal(spectro.GetIntensityData());
            return spectrum;
        }

        internal static StatisticPod GetSignalStatisticsArray(double[] data)
        {
            StatisticPod stats = new StatisticPod();
            for (int i = 0; i < data.Length; i++)
            {
                stats.Update(data[i]);
            }
            return stats;
        }
    }
}
