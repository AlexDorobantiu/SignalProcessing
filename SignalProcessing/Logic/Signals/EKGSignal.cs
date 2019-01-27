using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SignalProcessing.Logic.Signals
{
    public class EKGSignal : Signal
    {
        private readonly string filename;
        public string Filename { get { return filename; } }

        int[] data;
        int index;

        public override int NumberOfChannels
        {
            get
            {
                return 1;
            }
            set
            {
            }
        }

        public EKGSignal(string filename)
        {
            this.filename = filename;
            StreamReader sr = new StreamReader(filename);
            char[] separator = new char[1] { ' ' };
            string[] s = sr.ReadLine().Split(separator);

            // the following are default values taken from sample files
            numberOfSamples = 1100;
            sampleRate = 100;
            BitsPerSample = 16;

            data = new int[NumberOfSamples];

            for (int i = 0; i < NumberOfSamples; i++)
            {
                data[i] = short.Parse(s[i]);
            }

            index = 0;
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
            if (NumberOfSamples - index < chunkSize)
            {
                copySize = NumberOfSamples - index;
            }
            for (int i = 0; i < copySize; i++)
            {
                chunk[i] = data[index++];
            }
            return copySize;
        }
    }
}