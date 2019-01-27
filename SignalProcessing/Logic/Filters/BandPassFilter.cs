using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SignalProcessing.Logic.Signals;

namespace SignalProcessing.Logic.Filters
{
    class BandPassFilter : Filter
    {
        private static PropertyChangedEventArgs BandLowPropertyChangedEventArgs = new PropertyChangedEventArgs("BandLow");
        private static PropertyChangedEventArgs BandHighPropertyChangedEventArgs = new PropertyChangedEventArgs("BandHigh");

        public double BandLow
        {
            get { return bandLow; }
            set
            {
                bandLow = value;
                if (bandLow > bandHigh)
                {
                    BandHigh = bandLow;
                }
                notifyPropertyChanged(BandLowPropertyChangedEventArgs);
            }
        }

        public double BandHigh
        {
            get { return bandHigh; }
            set
            {
                bandHigh = value;
                if (bandLow > bandHigh)
                {
                    BandLow = bandHigh;
                }
                notifyPropertyChanged(BandHighPropertyChangedEventArgs);
            }
        }

        private double bandLow;
        private double bandHigh;
        double[] convolutionKernel;

        public BandPassFilter(int n, double bandLow, double bandHigh)
        {
            this.n = n;
            this.bandLow = bandLow;
            this.bandHigh = bandHigh;
            convolutionKernel = new double[n];
        }

        public override Signal FilterSignal(Signal signal)
        {
            GenericSignal resultSignal = new GenericSignal(signal.SampleRate, signal.NumberOfChannels, signal.NumberOfSamples);
            if (convolutionKernel.Length != n) convolutionKernel = new double[n];

            double fe = 2 * Math.PI * signal.SampleRate;

            for (int k = -(n - 1) / 2; k <= (n - 1) / 2; k++)
            {
                if (k == 0)
                {
                    convolutionKernel[k + (n - 1) / 2] = 2 / fe * (bandHigh - bandLow);
                }
                else
                {
                    double y = k * 2 * Math.PI / fe;
                    convolutionKernel[k + (n - 1) / 2] = (2 / fe) * ((bandHigh * Math.Sin(bandHigh * y) / (bandHigh * y)) - (bandLow * Math.Sin(bandLow * y) / (bandLow * y)));
                }
            }

            for (int c = 0; c < signal.NumberOfChannels; c++)
            {
                resultSignal.GoToSample(c, n);
                for (int i = n; i < signal.NumberOfSamples; i++)
                {
                    signal.GoToSample(c, i - n);

                    double sum = 0;
                    for (int k = 0; k < n; k++)
                        sum += convolutionKernel[k] * signal.GetSample(c);

                    if (sum > short.MaxValue)
                    {
                        sum = short.MaxValue;
                    }
                    if (sum < short.MinValue)
                    {
                        sum = short.MinValue;
                    }
                    resultSignal.SetSample(c, (int)sum);
                }
            }

            return resultSignal;
        }


    }
}
