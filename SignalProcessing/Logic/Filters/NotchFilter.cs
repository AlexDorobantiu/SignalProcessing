using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SignalProcessing.Logic.Util;
using SignalProcessing.Logic.Signals;

namespace SignalProcessing.Logic.Filters
{
    class NotchFilter : Filter
    {
        public static readonly double DefaultR1 = 0.9d;
        public static readonly double MaximumR1 = 0.99d;
        public static readonly double MinimumR1 = 0.01d;

        public static readonly double DefaultR2 = 0.8d;
        public static readonly double MaximumR2 = 0.99d;
        public static readonly double MinimumR2 = 0.01d;

        private static PropertyChangedEventArgs ThresholdFreqPropertyChangedEventArgs = new PropertyChangedEventArgs("ThresholdFreq");
        private static PropertyChangedEventArgs R1PropertyChangedEventArgs = new PropertyChangedEventArgs("R1");
        private static PropertyChangedEventArgs R2PropertyChangedEventArgs = new PropertyChangedEventArgs("R2");

        public double ThresholdFreq
        {
            get { return frequencyThresold; }
            set
            {
                frequencyThresold = value;
                notifyPropertyChanged(ThresholdFreqPropertyChangedEventArgs);
            }
        }

        public double R1
        {
            get { return r1; }
            set
            {
                r1 = value;
                if (r1 > MaximumR1)
                {
                    r1 = MaximumR1;
                }
                if (r1 < MinimumR1)
                {
                    r1 = MinimumR1;
                }
                notifyPropertyChanged(R1PropertyChangedEventArgs);
            }
        }

        public double R2
        {
            get { return r2; }
            set
            {
                r2 = value;
                if (r2 > MaximumR2)
                {
                    r2 = MaximumR2;
                }
                if (r2 < MinimumR2)
                {
                    r2 = MinimumR2;
                }
                notifyPropertyChanged(R2PropertyChangedEventArgs);
            }
        }

        private double r1, r2;
        private double frequencyThresold;

        public NotchFilter(double r1, double r2, double frequencyThresold)
        {
            this.r1 = r1;
            this.r2 = r2;
            this.frequencyThresold = frequencyThresold;
        }

        public override Signal FilterSignal(Signal signal)
        {
            GenericSignal resultSignal = new GenericSignal(signal.SampleRate, signal.NumberOfChannels, signal.NumberOfSamples);

            double fe = 2 * Math.PI * signal.SampleRate;
            double fi = 2 * Math.PI * frequencyThresold / fe;

            ComplexNumber z = new ComplexNumber(Math.Cos(fi), Math.Sin(fi));
            ComplexNumber z1 = new ComplexNumber(z.real, z.imaginary);
            ComplexNumber z2 = ComplexNumber.conjugate(z1);

            ComplexNumber p1 = new ComplexNumber(r1 * Math.Cos(fi), r1 * Math.Sin(fi));
            ComplexNumber p2 = ComplexNumber.conjugate(p1);

            ComplexNumber p3 = new ComplexNumber(r2 * Math.Cos(fi), r2 * Math.Sin(fi));
            ComplexNumber p4 = ComplexNumber.conjugate(p3);

            double a0 = ComplexNumber.module(ComplexNumber.multiply(ComplexNumber.substract(z, p1), ComplexNumber.substract(z, p2)));
            double a1 = ComplexNumber.module(ComplexNumber.multiply(ComplexNumber.substract(z, p2), ComplexNumber.substract(z, p3)));

            for (int c = 0; c < signal.NumberOfChannels; c++)
            {
                signal.GoToSample(c, 0);
                resultSignal.GoToSample(c, 2);

                int last1 = 0, last2 = 0;
                int last2X = signal.GetSample(c);
                int last1X = signal.GetSample(c);

                for (int i = 2; i < signal.NumberOfSamples; i++)
                {
                    int x = signal.GetSample(c);
                    int value = (int)(a0 * x + a0 * r2 * z1.real / 9 * last1X + 2 * r1 * z.real * last1 - r1 * r1 * last2);

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

                    last2X = last1X;
                    last1X = x;
                }
            }

            return resultSignal;
        }
    }
}
