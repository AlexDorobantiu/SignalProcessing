using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using SignalProcessing.Logic.Exceptions;

namespace SignalProcessing.Logic.Signals
{
    class GenericSignal : Signal
    {
        private int[][] data;
        private int[] channelIndex;

        public GenericSignal(int sampleRate, int numberOfChannels, int numberOfSamples)
        {
            BitsPerSample = 16;
            this.sampleRate = sampleRate;
            this.numberOfSamples = numberOfSamples;
            NumberOfChannels = numberOfChannels;

            data = new int[NumberOfChannels][];
            for (int i = 0; i < NumberOfChannels; i++)
            {
                data[i] = new int[NumberOfSamples];
            }

            channelIndex = new int[NumberOfChannels];
        }

        public void SetSample(int ChannelNumber, int sample)
        {
            data[ChannelNumber][channelIndex[ChannelNumber]++] = sample;
            if (sample > MaxSample)
            {
                MaxSample = sample;
            }
            if (sample < -MaxSample)
            {
                MaxSample = -sample;
            }
        }

        public void SetChunk(int ChannelNumber, int ChunkSize, int[] chunk)
        {
            for (int i = 0; i < ChunkSize; i++)
            {
                data[ChannelNumber][channelIndex[ChannelNumber]++] = chunk[i];
                if (chunk[i] > MaxSample)
                {
                    MaxSample = chunk[i];
                }
                if (chunk[i] < -MaxSample)
                {
                    MaxSample = -chunk[i];
                }
            }
        }

        public override int[] GetChannel(int channelNumber)
        {
            return data[channelNumber];
        }

        public override void GoToSample(int channelNumber, int sampleNumber)
        {
            channelIndex[channelNumber] = sampleNumber;
        }

        public override int GetSample(int channelNumber)
        {
            return (data[channelNumber][channelIndex[channelNumber]++]);
        }

        public override int GetChunk(int channelNumber, int chunkSize, int[] chunk)
        {
            int copySize = chunkSize;
            if ((NumberOfSamples - channelIndex[channelNumber]) < chunkSize)
            {
                copySize = NumberOfSamples - channelIndex[channelNumber];
            }
            for (int i = 0; i < copySize; i++)
            {
                chunk[i] = data[channelNumber][channelIndex[channelNumber]++];
            }
            return copySize;
        }

        public static GenericSignal FromSum(IEnumerable<Signal> sources)
        {
            int maxNrSamples = -1;
            int maxNrChannels = -1;
            int sampleRate = 0;

            foreach (Signal sig in sources)
            {
                if (sig.NumberOfSamples > maxNrSamples)
                {
                    maxNrSamples = sig.NumberOfSamples;
                }
                if (sig.NumberOfChannels > maxNrChannels)
                {
                    maxNrChannels = sig.NumberOfChannels;
                }
                sampleRate = sig.SampleRate;
                for (int c = 0; c < sig.NumberOfChannels; c++)
                {
                    sig.GoToSample(c, 0); //reset all sample indexes
                }
            }

            if (maxNrSamples == -1) // no sources
            {
                return new GenericSignal(2, 1, 2);
            }

            //check sample rates to be equal
            foreach (Signal sig in sources)
            {
                if (sig.SampleRate != sampleRate)
                {
                    throw new IncompatibleSampleRatesException();
                }
            }

            int maxValue = 0;

            GenericSignal genericSignal = new GenericSignal(sampleRate, maxNrChannels, maxNrSamples);

            // sum of all the signals
            
            for (int i = 0; i < maxNrSamples; i++)
            {
                for (int c = 0; c < maxNrChannels; c++)
                {
                    int sum = 0;
                    foreach (Signal sig in sources)
                    {
                        if ((c < sig.NumberOfChannels) && (i < sig.NumberOfSamples))
                        {
                            sum += sig.GetSample(c);
                        }
                    }

                    if (sum > short.MaxValue)
                    {
                        sum = short.MaxValue;
                    }
                    if (sum < short.MinValue)
                    {
                        sum = short.MinValue;
                    }

                    genericSignal.SetSample(c, sum);

                    if (sum > maxValue)
                    {
                        maxValue = sum;
                    }
                    if (-sum > maxValue)
                    {
                        maxValue = -sum;
                    }
                }
            }

            genericSignal.MaxSample = maxValue; // take the maximum value for rescaling

            return genericSignal;
        }
    }
}
