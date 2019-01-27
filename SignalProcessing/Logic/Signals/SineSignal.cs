using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace SignalProcessing.Logic.Signals
{
    public class SineSignal : ResizableSignal, INotifyPropertyChanged
    {
        public static readonly int DefaultSamplingRate = 8000;
        public static readonly int DefaultNumberOfSamples = 1000;
        public static readonly double DefaultFrequency = 2000;
        public static readonly int DefaultAmplitude = 1000;

        private bool upToDate = false;

        public new event PropertyChangedEventHandler PropertyChanged;
        private static PropertyChangedEventArgs AmplitudePropertyChangedEventArgs = new PropertyChangedEventArgs("Amplitude");
        private static PropertyChangedEventArgs FrequencyPropertyChangedEventArgs = new PropertyChangedEventArgs("Frequency");
        private static PropertyChangedEventArgs InitialPhasePropertyChangedEventArgs = new PropertyChangedEventArgs("InitialPhase");
        private static PropertyChangedEventArgs SampleRatePropertyChangedEventArgs = new PropertyChangedEventArgs("SampleRate");
        private static PropertyChangedEventArgs MaxFrequencyPropertyChangedEventArgs = new PropertyChangedEventArgs("MaxFrequency");
        private static PropertyChangedEventArgs TimeLengthPropertyChangedEventArgs = new PropertyChangedEventArgs("TimeLength");
        private static PropertyChangedEventArgs NumberOfSamplesPropertyChangedEventArgs = new PropertyChangedEventArgs("NumberOfSamples");

        public override bool IsEnabled
        {
            get { return isEnabled; }
            set { isEnabled = value; if (PropertyChanged != null) PropertyChanged(this, IsEnabledPropertyChangedEventArgs); }
        }

        private Int32 amplitude;
        public Int32 Amplitude
        {
            get { return amplitude; }
            set
            {
                if (value != amplitude)
                {
                    upToDate = false;
                }
                amplitude = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, AmplitudePropertyChangedEventArgs);
                }
            }
        }
        private double frequency;
        public double Frequency
        {
            get { return frequency; }
            set
            {
                if (value != frequency)
                {
                    upToDate = false;
                }
                frequency = value;
                if (frequency > MaxFrequency)
                {
                    frequency = MaxFrequency;
                }
                if (frequency < 1)
                {
                    frequency = 1;
                }
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, FrequencyPropertyChangedEventArgs);
                }
            }
        }
        private double initialPhase;
        public double InitialPhase
        {
            get { return initialPhase; }
            set
            {
                if (value != initialPhase)
                {
                    upToDate = false;
                }
                initialPhase = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, InitialPhasePropertyChangedEventArgs);
                }
            }
        }

        public override int NumberOfChannels { get { return 1; } }

        public double MaxFrequency
        {
            get { return sampleRate / 2; }
        }

        private int[] data;
        private int index;

        public SineSignal()
        {
            sampleRate = DefaultSamplingRate;
            numberOfSamples = DefaultNumberOfSamples;
            frequency = DefaultFrequency;
            amplitude = DefaultAmplitude;

            BitsPerSample = 16;
            initialPhase = 0;

            index = 0;
        }

        public override void SetSampleRate(int sampleRate)
        {
            if (sampleRate != this.sampleRate) upToDate = false;
            this.sampleRate = sampleRate;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, SampleRatePropertyChangedEventArgs);
                PropertyChanged(this, MaxFrequencyPropertyChangedEventArgs);
                PropertyChanged(this, TimeLengthPropertyChangedEventArgs);
            }

            if (frequency > MaxFrequency) frequency = MaxFrequency;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, FrequencyPropertyChangedEventArgs);
            }
        }

        public override void SetNumberOfSamples(int numberOfSamples)
        {
            if (numberOfSamples != this.numberOfSamples) upToDate = false;
            this.numberOfSamples = numberOfSamples;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, TimeLengthPropertyChangedEventArgs);
                PropertyChanged(this, NumberOfSamplesPropertyChangedEventArgs);
            }
        }

        public override void ResizeUpdate()
        {
            Update();
        }

        public void Update()
        {
            if (upToDate)
            {
                return;
            }

            int maxData = 0;
            int minData = 0;

            if ((data == null) || (data.Length != numberOfSamples))
            {
                data = new int[numberOfSamples];
            }

            double t = 0;
            double step = 1.0 / SampleRate;

            for (int i = 0; i < numberOfSamples; i++)
            {
                data[i] = (int)(Amplitude * Math.Sin(Frequency * t + InitialPhase));
                t += step;
                if (data[i] > maxData)
                {
                    maxData = data[i];
                }
                if (data[i] < minData)
                {
                    minData = data[i];
                }
            }

            index = 0;

            MaxSample = (maxData > -minData) ? maxData : -minData;

            upToDate = true;
        }

        public override int[] GetChannel(int channelNumber)
        {
            return data;
        }

        public override void GoToSample(int channelNumber, int sampleNumber)
        {
            index = sampleNumber;
        }

        public override int GetSample(int channelNumber)
        {
            return data[index++];
        }

        public override int GetChunk(int channelNumber, int chunkSize, int[] chunk)
        {
            int copySize = chunkSize;
            if (NumberOfSamples - index < chunkSize) copySize = NumberOfSamples - index;
            for (int i = 0; i < copySize; i++)
            {
                chunk[i] = data[index++];
            }
            return copySize;
        }
    }
}
