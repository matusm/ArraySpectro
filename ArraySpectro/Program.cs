using At.Matus.OpticalSpectrumLib;
using At.Matus.StatisticPod;
using Bev.Instruments.Thorlabs.Ctt;
using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ArraySpectro
{
    internal partial class Program
    {
        private static ThorlabsCct spectro;
        private static StreamWriter logFile;
        private static StatisticPod temperature;

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

            double testIntTime = 5; // seconds
            int hwAveraging = 1;
            int nSamples = 10;
            int nCycles = 100;
            int idleTimeMs = 100000; // 100 seconds
            string logFileName = $"2SacrificialABBAStabilityLog_{nSamples}x{(int)(testIntTime * 1000)}ms.csv";
            logFile = new StreamWriter(logFileName, false);
            DateTime logStartTime = DateTime.Now;

            spectro.SetIntegrationTime(testIntTime);
            spectro.SetHardwareAveraging(hwAveraging);
            MeasuredOpticalSpectrum sacrificialSpectrum = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            MeasuredOpticalSpectrum specA = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            MeasuredOpticalSpectrum specB = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            temperature = new StatisticPod();
            for (int cycle = 0; cycle < nCycles; cycle++)
            {
                Console.WriteLine();
                Console.WriteLine($"{logFileName} : Cycle {cycle + 1} of {nCycles}");
                specA.Clear();
                specB.Clear();
                temperature.Restart();
                temperature.Update(spectro.Temperature);
                OnCallUpdateSpectrum(sacrificialSpectrum, nSamples, $"- Sacrificial Spectrum 1 -");
                temperature.Update(spectro.Temperature);
                OnCallUpdateSpectrum(sacrificialSpectrum, nSamples, $"- Sacrificial Spectrum 2 -");
                spectro.OpenShutter();
                temperature.Update(spectro.Temperature);
                OnCallUpdateSpectrum(specA, nSamples, $"Signal Spectrum A_1");
                spectro.CloseShutter();
                temperature.Update(spectro.Temperature);
                OnCallUpdateSpectrum(specB, nSamples, $"Dark Spectrum B_1");
                spectro.CloseShutter();
                temperature.Update(spectro.Temperature);
                OnCallUpdateSpectrum(specB, nSamples, $"Dark Spectrum B_2");
                spectro.OpenShutter();
                temperature.Update(spectro.Temperature);
                OnCallUpdateSpectrum(specA, nSamples, $"Signal Spectrum A_2");
                temperature.Update(spectro.Temperature);
                var specABBA = SpecMath.Subtract(specA, specB);
                string logTimeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string logLine = $"{logTimeStamp}, {(DateTime.Now - logStartTime).TotalSeconds:F0}, {temperature.AverageValue:F2},  {temperature.Range:F2}, {specABBA.GetSignalStatistics().AverageValue:F2}, {specABBA.GetSignalStatistics().StandardDeviation:F2}, {specB.GetSignalStatistics().AverageValue:F2}, {specB.GetSignalStatistics().StandardDeviation:F2}";
                Console.WriteLine(logLine);
                logFile.WriteLine(logLine);
                logFile.Flush();
                Console.WriteLine($"Waiting for {idleTimeMs / 1000} seconds before next cycle...");
                Thread.Sleep(idleTimeMs);
            }

        }


        internal static MeasuredOpticalSpectrum GetSpectrum()
        {
            MeasuredOpticalSpectrum spectrum = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            spectrum.UpdateSignal(spectro.GetIntensityData());
            return spectrum;
        }

        //internal static StatisticPod GetSignalStatisticsArray(double[] data)
        //{
        //    StatisticPod stats = new StatisticPod();
        //    for (int i = 0; i < data.Length; i++)
        //    {
        //        stats.Update(data[i]);
        //    }
        //    return stats;
        //}
    }
}
