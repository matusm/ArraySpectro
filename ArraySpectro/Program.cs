using At.Matus.OpticalSpectrumLib;
using Bev.Instruments.Thorlabs.Ctt;
using System;
using System.Globalization;
using System.IO;

namespace ArraySpectro
{
    internal partial class Program
    {
        private static ThorlabsCct spectro;

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
            Console.WriteLine();

            //Console.WriteLine("Estimating optimal integration time...");
            //double optimalIntegrationTime = spectro.GetOptimalExposureTime();
            //Console.WriteLine($"Optimal Integration Time: {optimalIntegrationTime} s");
            //Console.WriteLine();
            //spectro.SetIntegrationTime(optimalIntegrationTime);

            spectro.SetIntegrationTime(1.0);

            int nSamples = 1800;

            MeasuredOpticalSpectrum reference = new MeasuredOpticalSpectrum(spectro.Wavelengths);
            MeasuredOpticalSpectrum dark = new MeasuredOpticalSpectrum(spectro.Wavelengths);

            for (int i = 0; i < 3; i++)
            {
                spectro.OpenShutter();
                OnCallUpdateSpectrum(reference, nSamples, "Reference Spectrum");
                spectro.CloseShutter();
                OnCallUpdateSpectrum(dark, nSamples, "Dark Spectrum");
            }
            spectro.OpenShutter();

            OpticalSpectrum corrected = SpecMath.Subtract(reference, dark);

            string fileName = $"LowLight_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string outPath = Path.Combine(Environment.CurrentDirectory, fileName);
            corrected.WriteToCsvFile(outPath);

            Console.WriteLine("done.");


        }
    }
}
