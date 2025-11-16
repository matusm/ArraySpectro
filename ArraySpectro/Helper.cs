using At.Matus.OpticalSpectrumLib;
using Bev.Instruments.Thorlabs.Ctt;
using System;
using System.Text;

namespace ArraySpectro
{
    internal static class Helper
    {

        // Updates the provided MeasuredSpectrum by taking numberSamples scans from the tlCcs device.
        internal static void UpdateSpectrumUI(MeasuredOpticalSpectrum spectrum, int numberSamples, string message, ThorlabsCct spectro)
        {
            Console.WriteLine($"Press any key to start measurement of {message} - 's' to skip");
            if (Console.ReadKey(true).Key != ConsoleKey.S)
            {
                ConsoleProgressBar consoleProgressBar = new ConsoleProgressBar();
                for (int i = 0; i < numberSamples; i++)
                {
                    spectrum.UpdateSignal(spectro.GetIntensityData());
                    consoleProgressBar.Report(i + 1, numberSamples);
                }
                spectrum.Name = $"{message}";
            }
            else
            {
                Console.WriteLine($"Skipping measurement of {message}");
            }
        }

        // Creates and returns a MeasuredSpectrum by taking numberSamples scans from the tlCcs device.
        internal static MeasuredOpticalSpectrum GetSpectrumUI(int numberSamples, string message, ThorlabsCct spectro)
        {
            MeasuredOpticalSpectrum spectrum = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            UpdateSpectrumUI(spectrum, numberSamples, message, spectro);
            return spectrum;
        }

        internal static string ToCsvLines(this IOpticalSpectrum spectrum)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < spectrum.NumberOfPoints; i++)
            {
                sb.AppendLine(spectrum.DataPoints[i].ToCsvLine());
            }
            return sb.ToString();
        }

    }

}
