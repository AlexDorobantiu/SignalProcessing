using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using SignalProcessing.Logic.Signals;

namespace SignalProcessing.Logic.Filters
{
    class AverageFilter : Filter, INotifyPropertyChanged
    {
        public AverageFilter(int n)
        {
            this.n = n;
        }

        public override Signal FilterSignal(Signal signal)
        {
            GenericSignal resultSignal = new GenericSignal(signal.SampleRate, signal.NumberOfChannels, signal.NumberOfSamples);

            for (int c = 0; c < signal.NumberOfChannels; c++)
            {
                resultSignal.GoToSample(c, n);
            }

            for (int i = n; i < signal.NumberOfSamples; i++)
            {

                for (int c = 0; c < signal.NumberOfChannels; c++)
                {
                    signal.GoToSample(c, i - n);

                    double sum = 0;
                    for (int k = 0; k < n; k++)
                    {
                        sum += signal.GetSample(c);
                    }

                    sum /= n;

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
