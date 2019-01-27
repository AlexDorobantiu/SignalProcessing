using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SignalProcessing.Logic.Signals
{
    class WavSignal : Signal
    {
        private readonly string filename;
        public string Filename { get { return filename; } }

        private static readonly int BitsPerByte = 8;
        private static readonly int MaxBits = 16;

        private int[][] data;
        private int[] channelIndex;

        public WavSignal(string fileName)
        {
            int maxdata = 0;
            int mindata = 0;

            filename = fileName;
            FileStream stream = new FileStream(fileName, FileMode.Open);
            using (BinaryReader binaryReader = new BinaryReader(stream))
            {
                binaryReader.ReadChars(4); // "RIFF"
                int length = binaryReader.ReadInt32();
                binaryReader.ReadChars(4); // "WAVE"
                string chunkName = new string(binaryReader.ReadChars(4)); //"fmt "
                int chunkLength = binaryReader.ReadInt32();
                int compressionCode = binaryReader.ReadInt16(); //1 for PCM/uncompressed
                NumberOfChannels = binaryReader.ReadInt16();
                sampleRate = binaryReader.ReadInt32();
                int bytesPerSecond = binaryReader.ReadInt32();
                int blockAlign = binaryReader.ReadInt16();
                BitsPerSample = binaryReader.ReadInt16();
                if ((MaxBits % BitsPerSample) != 0)
                {
                    throw new Exception("The input stream uses an unhandled SignificantBitsPerSample parameter");
                }
                binaryReader.ReadChars(chunkLength - 16);
                chunkName = new string(binaryReader.ReadChars(4));
                try
                {
                    while (chunkName.ToLower() != "data")
                    {
                        binaryReader.ReadChars(binaryReader.ReadInt32());
                        chunkName = new string(binaryReader.ReadChars(4));
                    }
                }
                catch
                {
                    throw new Exception("Input stream misses the data chunk");
                }
                chunkLength = binaryReader.ReadInt32();
                int frames = 8 * chunkLength / BitsPerSample / NumberOfChannels;
                numberOfSamples = frames;
                data = new int[NumberOfChannels][];
                channelIndex = new int[NumberOfChannels]; //for channel indexing

                for (int channel = 0; channel < NumberOfChannels; channel++)
                {
                    data[channel] = new int[frames];
                }
                int readedBits = 0;
                int numberOfReadedBits = 0;
                for (int frame = 0; frame < frames; frame++)
                {
                    for (int channel = 0; channel < NumberOfChannels; channel++)
                    {
                        while (numberOfReadedBits < BitsPerSample)
                        {
                            readedBits |= Convert.ToInt32(binaryReader.ReadByte()) << numberOfReadedBits;
                            numberOfReadedBits += BitsPerByte;
                        }
                        int numberOfExcessBits = numberOfReadedBits - BitsPerSample;
                        if (BitsPerSample == 8)
                        {
                            data[channel][frame] = ((char)(readedBits >> numberOfExcessBits)) * 256;
                        }
                        else
                        {
                            if (BitsPerSample == 16)
                            {
                                data[channel][frame] = (short)(readedBits >> numberOfExcessBits);
                            }
                            else
                            {
                                data[channel][frame] = readedBits >> numberOfExcessBits;
                            }
                        }

                        if (data[channel][frame] > maxdata)
                        {
                            maxdata = data[channel][frame];
                        }
                        if (data[channel][frame] < mindata)
                        {
                            mindata = data[channel][frame];
                        }

                        readedBits = readedBits % (1 << numberOfExcessBits);
                        numberOfReadedBits = numberOfExcessBits;
                    }
                }

                BitsPerSample = 16; // we do an automatic conversion of 8bit wav to 16bit
                MaxSample = (maxdata > -mindata) ? maxdata : -mindata;
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
    }
}
