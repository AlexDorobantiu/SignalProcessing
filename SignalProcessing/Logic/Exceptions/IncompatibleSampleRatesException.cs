using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalProcessing.Logic.Exceptions
{
    public class IncompatibleSampleRatesException : Exception
    {
        public IncompatibleSampleRatesException() :
            base("You tried to perform operations on signals of different sample rates. This is not supported.")
        {
        }

        public IncompatibleSampleRatesException(string message) :
            base(message)
        {
        }
    }
}
