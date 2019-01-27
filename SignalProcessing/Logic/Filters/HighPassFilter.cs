using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SignalProcessing.Logic.Signals;

namespace SignalProcessing.Logic.Filters
{
    class HighPassFilter : Filter
    {
        private static PropertyChangedEventArgs ThresholdFreqPropertyChangedEventArgs = new PropertyChangedEventArgs("ThresholdFreq");

        public double ThresholdFreq
        {
            get { return frequencyThreshold; }
            set
            {
                frequencyThreshold = value;
                notifyPropertyChanged(ThresholdFreqPropertyChangedEventArgs);
            }
        }

        private double frequencyThreshold;
        double[] convolutionKernel;

        public HighPassFilter(int n, double frequencyThreshold)
        {
            this.n = n;
            this.frequencyThreshold = frequencyThreshold;
            convolutionKernel = new double[n];
        }

        public override Signal FilterSignal(Signal signal)
        {
            GenericSignal resultSignal = new GenericSignal(signal.SampleRate, signal.NumberOfChannels, signal.NumberOfSamples);
            if (convolutionKernel.Length != n)
            {
                convolutionKernel = new double[n];
            }

            double fe = 2 * Math.PI * signal.SampleRate;
            double z = 2 * frequencyThreshold / fe;

            for (int k = -(n - 1) / 2; k <= (n - 1) / 2; k++)
            {
                if (k == 0)
                {
                    convolutionKernel[k + (n - 1) / 2] = 1 - z;
                }
                else
                {
                    double pik = Math.PI * k;
                    double y = z * pik;
                    convolutionKernel[k + (n - 1) / 2] = (Math.Sin(pik) / pik) - z * (Math.Sin(y) / (y));
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
                    {
                        sum += convolutionKernel[k] * signal.GetSample(c);
                    }

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