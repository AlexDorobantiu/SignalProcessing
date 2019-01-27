using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing.Logic.Signals
{
    public abstract class ResizableSignal : Signal
    {
        public abstract void SetSampleRate(int sampleRate);
        public abstract void SetNumberOfSamples(int numberOfSamples);
        public abstract void ResizeUpdate();
    }
}
