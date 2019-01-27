using System.Collections.Generic;
using System.ComponentModel;
using SignalProcessing.Logic.Signals;

namespace SignalProcessing.Logic.Filters
{
    public abstract class Filter : INotifyPropertyChanged
    {
        public static readonly int DefaultN = 150;

        public virtual event PropertyChangedEventHandler PropertyChanged;

        private static PropertyChangedEventArgs NPropertyChangedEventArgs = new PropertyChangedEventArgs("N");
        private static PropertyChangedEventArgs IsEnabledPropertyChangedEventArgs = new PropertyChangedEventArgs("IsEnabled");

        protected void notifyPropertyChanged(PropertyChangedEventArgs arg)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, arg);
            }
        }

        protected int n;
        public int N
        {
            get { return n; }
            set
            {
                n = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, NPropertyChangedEventArgs);
                }
            }
        }

        protected bool isEnabled;
        public bool IsEnabled
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

        public Filter()
        {
            n = DefaultN;
            isEnabled = true;
        }

        public abstract Signal FilterSignal(Signal signal);

        public static Signal GetFiltered(Signal source, IEnumerable<Filter> filters)
        {
            if (filters == null)
            {
                return source;
            }

            Signal result = null;

            // apply all filters
            foreach (Filter filter in filters)
            {
                result = filter.FilterSignal(source);
                source = result;
            }

            if (result == null)
            {
                return source;
            }

            return result;
        }
    }
}
