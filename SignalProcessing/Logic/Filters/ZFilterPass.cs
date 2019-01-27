using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SignalProcessing.Logic.Util;
using SignalProcessing.Logic.Signals;

namespace SignalProcessing.Logic.Filters
{
    class ZFilterPass : Filter
    {
        public static readonly double DefaultR = 0.9d;
        public static readonly double MaximumR = 0.999d;
        public static readonly double MinimumR = 0.001d;

        private static PropertyChangedEventArgs ThresholdFreqPropertyChangedEventArgs = new PropertyChangedEventArgs("ThresholdFreq");
        private static PropertyChangedEventArgs RPropertyChangedEventArgs = new PropertyChangedEventArgs("R");

        public double ThresholdFreq
        {
            get { return frequencyThreshold; }
            set
            {
                frequencyThreshold = value;
                notifyPropertyChanged(ThresholdFreqPropertyChangedEventArgs);
            }
        }

        public double R
        {
            get { return r; }
            set
            {
                r = value;
                if (r > MaximumR)
                {
                    r = MaximumR;
                }
                if (r < MinimumR)
                {
                    r = MinimumR;
                }
                notifyPropertyChanged(RPropertyChangedEventArgs);
            }
        }

        private double r;
        private double frequencyThreshold;

        public ZFilterPass(double r, double frequencyThreshold)
        {
            this.r = r;
            this.frequencyThreshold = frequencyThreshold;
        }

        public override Signal FilterSignal(Signal signal)
        {
            GenericSignal resultSignal = new GenericSignal(signal.SampleRate, signal.NumberOfChannels, signal.NumberOfSamples);

            double fe = 2 * Math.PI * signal.SampleRate;
            double fi = 2 * Math.PI * frequencyThreshold / fe;

            ComplexNumber z = new ComplexNumber(Math.Cos(fi), Math.Sin(fi));
            ComplexNumber p1 = new ComplexNumber(r * Math.Cos(fi), r * Math.Sin(fi));
            ComplexNumber p2 = ComplexNumber.conjugate(p1);

            double a0 = ComplexNumber.module(ComplexNumber.multiply(ComplexNumber.substract(z, p1), ComplexNumber.substract(z, p2)));

            for (int c = 0; c < signal.NumberOfChannels; c++)
            {
                int last1 = 0, last2 = 0;

                resultSignal.GoToSample(c, 2);
                signal.GoToSample(c, 2);

                for (int i = 2; i < signal.NumberOfSamples; i++)
                {
                    int value = (int)(a0 * signal.GetSample(c) + 2 * r * z.real * last1 - r * r * last2);

                    if (value > short.MaxValue)
                    {
                        value = short.MaxValue;
                    }
                    if (value < short.MinValue)
                    {
                        value = short.MinValue;
                    }

                    resultSignal.SetSample(c, value);
                    last2 = last1;
                    last1 = value;
                }
            }

            return resultSignal;
        }
    }
}
