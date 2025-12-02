using At.Matus.OpticalSpectrumLib;
using System;

namespace ArraySpectro
{
    internal partial class Program
    {

        // Updates the provided MeasuredSpectrum by taking numberSamples scans from the tlCcs device, after a key press.
        internal static void OnKeyPressedUpdateSpectrum(MeasuredOpticalSpectrum spectrum, int numberSamples, string message)
        {
            Console.WriteLine($"Press any key to start measurement of {message} - 's' to skip");
            if (Console.ReadKey(true).Key != ConsoleKey.S)
            {
                OnCallUpdateSpectrum(spectrum, numberSamples, message);
            }
            else
            {
                Console.WriteLine($"Skipping measurement of {message}");
            }
        }

        // Creates and returns a MeasuredSpectrum by taking numberSamples scans from the tlCcs device after a key press.
        internal static MeasuredOpticalSpectrum OnKeyPressedGetSpectrum(int numberSamples, string message)
        {
            MeasuredOpticalSpectrum spectrum = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            OnKeyPressedUpdateSpectrum(spectrum, numberSamples, message);
            return spectrum;
        }

        internal static void OnCallUpdateSpectrum(MeasuredOpticalSpectrum spectrum, int numberSamples, string message)
        {
            Console.WriteLine($"Measurement of {message}...");
            ConsoleProgressBar consoleProgressBar = new ConsoleProgressBar();
            for (int i = 0; i < numberSamples; i++)
            {
                spectrum.UpdateSignal(spectro.GetIntensityData());
                consoleProgressBar.Report(i + 1, numberSamples);
            }
        }

        internal static MeasuredOpticalSpectrum OnCallGetSpectrum(int numberSamples, string message)
        {
            MeasuredOpticalSpectrum spectrum = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            OnCallUpdateSpectrum(spectrum, numberSamples, message);
            return spectrum;
        }

    }
}
