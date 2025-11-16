using At.Matus.OpticalSpectrumLib;
using Bev.Instruments.Thorlabs.Ctt;
using System;
using System.Globalization;
using System.IO;

namespace ArraySpectro
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var spectro = new ThorlabsCct();

            Console.WriteLine($"Instrument Manufacturer:  {spectro.InstrumentManufacturer}");
            Console.WriteLine($"Instrument Type:          {spectro.InstrumentType}");
            Console.WriteLine($"Instrument Serial Number: {spectro.InstrumentSerialNumber}");
            Console.WriteLine($"Firmware Revision:        {spectro.InstrumentFirmwareVersion}");
            Console.WriteLine($"Driver Revision:          {spectro.InstrumentElectronicsId}");
            Console.WriteLine($"Min Wavelength:           {spectro.MinimumWavelength:F2} nm");
            Console.WriteLine($"Max Wavelength:           {spectro.MaximumWavelength:F2} nm");
            Console.WriteLine();

            Console.WriteLine("Estimating optimal integration time...");
            double optimalIntegrationTime = spectro.GetOptimalExposureTime();
            Console.WriteLine($"Optimal Integration Time: {optimalIntegrationTime} s");
            Console.WriteLine();
            spectro.SetIntegrationTime(optimalIntegrationTime);

            int nSamples = 11;

            MeasuredOpticalSpectrum reference = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            MeasuredOpticalSpectrum dark = new MeasuredOpticalSpectrum(spectro.Wavelengths);

            spectro.OpenShutter();
            Helper.UpdateSpectrumUI(reference, nSamples, "Reference Spectrum", spectro);
            spectro.CloseShutter();
            Helper.UpdateSpectrumUI(dark, nSamples, "Dark Spectrum", spectro);
            spectro.OpenShutter();

            OpticalSpectrum corrected = SpecMath.Subtract(reference, dark);

            string csvString = corrected.ToCsvLines();
            string fileName = $"spectrum_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string outPath = Path.Combine(Environment.CurrentDirectory, fileName);
            File.WriteAllText(outPath, csvString);

            Console.WriteLine("done.");


        }
    }
}
