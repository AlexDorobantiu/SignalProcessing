using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using SignalProcessing.Logic.Signals;
using SignalProcessing.Logic.Filters;

namespace SignalProcessing
{
    /// <summary>
    /// Interaction logic for WindowMain.xaml
    /// </summary>
    public partial class WindowMain : Window, INotifyPropertyChanged
    {
        private ObservableCollection<Signal> sources = new ObservableCollection<Signal>();
        private ObservableCollection<Filter> filters = new ObservableCollection<Filter>();

        public event PropertyChangedEventHandler PropertyChanged;

        public WindowMain()
        {
            InitializeComponent();

            sourcesList.DataContext = sources;
            filtersList.DataContext = filters;

            updateSource();
        }

        private Signal sumOfAllSignals = null;
        private Signal filtered = null;

        private void updateSource()
        {
            var enabledSources = from source in sources
                                 where source.IsEnabled
                                 select source;

            sumOfAllSignals = GenericSignal.FromSum(enabledSources);
            SignalViewSource.SetRatioToFit(sumOfAllSignals);
            SignalViewSource.DrawGraphic(sumOfAllSignals, 0);
            updateResult();
        }

        private void updateResult()
        {
            var enabledFilters = from filter in filters
                                 where filter.IsEnabled
                                 select filter;

            filtered = Filter.GetFiltered(sumOfAllSignals, enabledFilters);
            SignalViewTarget.RatioY = SignalViewSource.RatioY;
            SignalViewTarget.DrawGraphic(filtered, 0);
        }

        private int globalSampleRate = 200;

        public int GlobalSampleRate
        {
            get { return globalSampleRate; }
            set
            {
                globalSampleRate = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GlobalSampleRate"));
                    PropertyChanged(this, new PropertyChangedEventArgs("GlobalMaxFrequency"));
                    PropertyChanged(this, new PropertyChangedEventArgs("GlobalTimeLength"));
                }
            }
        }

        private int globalNumberOfSamples = 200;

        public int GlobalNumberOfSamples
        {
            get { return globalNumberOfSamples; }
            set
            {
                globalNumberOfSamples = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GlobalNumberOfSamples"));
                    PropertyChanged(this, new PropertyChangedEventArgs("GlobalTimeLength"));
                }
            }
        }

        public double GlobalTimeLength { get { return (double)globalNumberOfSamples / globalSampleRate; } }

        public int GlobalMaxFrequency
        {
            get { return globalSampleRate / 2; }
        }

        private void btnAddSine_Click(object sender, RoutedEventArgs e)
        {
            SineSignal signal = new SineSignal();
            if (sources.Count > 0)
            {
                signal.SetSampleRate(sources[0].SampleRate);
                signal.SetNumberOfSamples(sources[0].NumberOfSamples);
            }
            else GlobalSampleRate = signal.SampleRate;
            signal.Update();

            sources.Add(signal);
            signal.PropertyChanged += new PropertyChangedEventHandler(sine_PropertyChanged);
            signal.PropertyChanged += new PropertyChangedEventHandler(signal_PropertyChanged);

            sourcesList.SelectedIndex = sources.IndexOf(signal);

            updateSource();
        }

        private bool suppressSourceUpdate = false;
        private void sine_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SineSignal sineSignal = sender as SineSignal;
            sineSignal.Update();
            if (!suppressSourceUpdate)
            {
                updateSource();
            }
        }

        void signal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsEnabled")
            {
                updateSource();
            }
        }

        private bool sourcesResizableOrSameSampleRate(int sampleRate)
        {
            foreach (var source in sources)
            {
                if (!((source is ResizableSignal) || (source.SampleRate == sampleRate)))
                {
                    return false;
                }
            }
            return true;
        }

        private void resizeResizableSourceSignals(int sampleRate, int numberOfSamples)
        {
            suppressSourceUpdate = true;
            foreach (var s in sources)
            {
                if (s is ResizableSignal)
                {
                    ResizableSignal resizableSignal = (s as ResizableSignal);
                    resizableSignal.SetSampleRate(sampleRate);
                    resizableSignal.SetNumberOfSamples(numberOfSamples);
                    resizableSignal.ResizeUpdate();
                }
            }
            suppressSourceUpdate = false;
        }

        private bool allSourcesAreResizableSignals()
        {
            foreach (Signal sig in sources)
            {
                if (!(sig is ResizableSignal))
                {
                    return false;
                }
            }

            return true;
        }

        private void btnAddWav_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultExt = "*.wav";
            openFileDialog.Filter = "Wave files (*.wav)|*wav";
            openFileDialog.ShowDialog();

            try
            {
                WavSignal wavSignal = new WavSignal(openFileDialog.FileName);
                if (sources.Count == 0)
                {
                    GlobalSampleRate = wavSignal.SampleRate;
                }
                else
                {
                    //check if we can actually add it
                    if (!sourcesResizableOrSameSampleRate(wavSignal.SampleRate))
                    {
                        return;
                    }
                    // resize the resizable signals
                    resizeResizableSourceSignals(wavSignal.SampleRate, wavSignal.NumberOfSamples);
                }

                wavSignal.PropertyChanged += new PropertyChangedEventHandler(signal_PropertyChanged);
                sources.Add(wavSignal);
                sourcesList.SelectedIndex = sources.IndexOf(wavSignal);

                updateSource();
            }
            catch
            {
            }
        }

        private void btnAddEKG_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.DefaultExt = "*.txt";
            openFileDialog.Filter = "EKG Files in Text Mode (*.txt)|*txt";
            openFileDialog.ShowDialog();

            try
            {
                EKGSignal ekgSignal = new EKGSignal(openFileDialog.FileName);
                if (sources.Count == 0)
                {
                    GlobalSampleRate = ekgSignal.SampleRate;
                }
                else
                {
                    // check if we can actually add it
                    if (!sourcesResizableOrSameSampleRate(ekgSignal.SampleRate))
                    {
                        return;
                    }

                    // resize the resizable signals
                    resizeResizableSourceSignals(ekgSignal.SampleRate, ekgSignal.NumberOfSamples);
                }

                ekgSignal.PropertyChanged += new PropertyChangedEventHandler(signal_PropertyChanged);
                sources.Add(ekgSignal);
                sourcesList.SelectedIndex = sources.IndexOf(ekgSignal);

                updateSource();
            }
            catch
            {
            }
        }

        private void TextBox_SR_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Signal signal = textBox.DataContext as Signal;

            if (allSourcesAreResizableSignals())
            {
                int value;
                if (int.TryParse(textBox.Text, out value))
                {
                    suppressSourceUpdate = true;
                    GlobalSampleRate = value;
                    foreach (Signal s in sources)
                    {
                        ResizableSignal resizableSignal = (s as ResizableSignal);
                        resizableSignal.SetSampleRate(value);
                        resizableSignal.ResizeUpdate();
                    }
                    suppressSourceUpdate = false;
                    updateSource();
                }
                else
                {
                    textBox.Text = signal.SampleRate.ToString();
                }
            }
            else
            {
                textBox.Text = signal.SampleRate.ToString();
            }
        }
        
        private void TextBox_NS_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Signal signal = textBox.DataContext as Signal;
            
            if (allSourcesAreResizableSignals())
            {
                int value;
                if (int.TryParse(textBox.Text, out value))
                {
                    suppressSourceUpdate = true;
                    GlobalNumberOfSamples = value;
                    foreach (Signal s in sources)
                    {
                        ResizableSignal resizableSignal = (s as ResizableSignal);
                        resizableSignal.SetNumberOfSamples(value);
                        resizableSignal.ResizeUpdate();
                    }
                    suppressSourceUpdate = false;
                    updateSource();
                }
                else textBox.Text = signal.SampleRate.ToString();
            }
            else
            {
                textBox.Text = signal.SampleRate.ToString();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            object dataContext = (sender as Button).DataContext;

            if (dataContext is Signal)
            {
                Signal signal = dataContext as Signal;
                sources.Remove(signal);
                if (signal is SineSignal)
                {
                    (signal as SineSignal).PropertyChanged -= new PropertyChangedEventHandler(sine_PropertyChanged);
                }
                updateSource();
            }

            if (dataContext is Filter)
            {
                Filter filter = (sender as Button).DataContext as Filter;
                filters.Remove(filter);
                filter.PropertyChanged -= new PropertyChangedEventHandler(Filter_PropertyChanged);

                updateResult();
            }
        }

        private void btnAddFilter_Click(object sender, RoutedEventArgs e)
        {
            Filter filter = null;

            switch ((sender as FrameworkElement).Name)
            {
                case "btnAddLPF": filter = new LowPassFilter(Filter.DefaultN, (double)globalSampleRate / 4); break;
                case "btnAddHPF": filter = new HighPassFilter(Filter.DefaultN, (double)globalSampleRate / 4); break;
                case "btnAddBPF": filter = new BandPassFilter(Filter.DefaultN, (double)globalSampleRate / 8, (double)globalSampleRate * 3 / 8); break;
                case "btnAddBSF": filter = new BandStopFilter(Filter.DefaultN, (double)globalSampleRate / 8, (double)globalSampleRate * 3 / 8); break;
                case "btnAddMF": filter = new AverageFilter(Filter.DefaultN); break;
                case "btnAddZPass": filter = new ZFilterPass(ZFilterPass.DefaultR, (double)globalSampleRate / 4); break;
                case "btnAddZPass1": filter = new ZFilterPass1(ZFilterPass1.DefaultR, (double)globalSampleRate / 4); break;
                case "btnAddNotch": filter = new NotchFilter(NotchFilter.DefaultR1, NotchFilter.DefaultR2, (double)globalSampleRate / 4); break;
                default: return;
            }

            filter.PropertyChanged += new PropertyChangedEventHandler(Filter_PropertyChanged);

            filters.Add(filter);
            filtersList.SelectedIndex = filters.IndexOf(filter);

            updateResult();
        }

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            updateResult();
        }
    }
}
