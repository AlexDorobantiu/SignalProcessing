using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SignalProcessing.Logic.Signals
{
    public abstract class Signal : INotifyPropertyChanged
    {
        protected int numberOfSamples;
        public int NumberOfSamples { get { return numberOfSamples; } }

        protected int sampleRate;
        public int SampleRate { get { return sampleRate; } }

        public virtual int NumberOfChannels { get; set; }
        public virtual int BitsPerSample { get; set; }
        public virtual int MaxSample { get; set; }
        public virtual double TimeLength { get { return (double)NumberOfSamples / SampleRate; } }

        public abstract int[] GetChannel(int channelNumber);
        public abstract void GoToSample(int channelNumber, int sampleNumber);
        public abstract int GetSample(int channelNumber);
        public abstract int GetChunk(int channelNumber, int chunkSize, int[] chunk);

        public event PropertyChangedEventHandler PropertyChanged;
        protected static PropertyChangedEventArgs IsEnabledPropertyChangedEventArgs = new PropertyChangedEventArgs("IsEnabled");
        protected bool isEnabled = true;
        public virtual bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                isEnabled = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, IsEnabledPropertyChangedEventArgs);
                }
            }
        }
    }

}
