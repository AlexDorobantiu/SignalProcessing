using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SignalProcessing.Logic.Signals;

namespace SignalProcessing
{
    /// <summary>
    /// Interaction logic for SignalView.xaml
    /// </summary>
    public partial class SignalView : UserControl
    {
        private static readonly Brush lineStroke = new SolidColorBrush(Colors.Blue);

        public SignalView()
        {
            InitializeComponent();
            Height = double.NaN;
            Width = double.NaN;
        }

        private double ratioY;
        public double RatioY
        {
            get { return ratioY; }
            set { ratioY = value; }
        }

        public void SetRatioToFit(int maxValue)
        {
            ratioY = MainCanvas.Height / 2 / (maxValue < 5 ? 5 : maxValue);
        }

        public void SetRatioToFit(Signal signal)
        {
            SetRatioToFit(signal.MaxSample);
        }


        //drawing ---------------------------------------------------------------------------------------------------
        private readonly PointCollection points = new PointCollection();
        private int drawMode = -1;
        private Polyline polyline = null;
        private Line[] verticalLines = null;

        public void DrawGraphic(Signal signal, int channelNumber)
        {
            double middleY = MainCanvas.Height / 2;
            signal.GoToSample(channelNumber, 0);

            // MODE 1
            if (signal.NumberOfSamples < MainCanvas.Width * 10)
            {
                double stepX = MainCanvas.Width / signal.NumberOfSamples;
                signal.GoToSample(channelNumber, 0);
                double x = 0;

                points.Clear();
                for (int i = 0; i < signal.NumberOfSamples; i++)
                {
                    int sample = signal.GetSample(channelNumber);
                    points.Add(new Point { X = x, Y = middleY - sample * ratioY });
                    x += stepX;
                }

                if (polyline == null)
                {
                    polyline = new Polyline();
                    polyline.Stroke = lineStroke;
                    polyline.Points = points;
                }

                if (drawMode != 1)
                {
                    MainCanvas.Children.Clear();
                    MainCanvas.Children.Add(polyline);
                }

                drawMode = 1;
            }
            // MODE 2
            else
            {
                int horizontalPixels = (int)MainCanvas.ActualWidth;

                if (verticalLines == null)
                {
                    verticalLines = new Line[horizontalPixels];
                    for (int pixel = 0; pixel < horizontalPixels; pixel++)
                    {
                        verticalLines[pixel] = new Line();
                        verticalLines[pixel].X1 = pixel;
                        verticalLines[pixel].X2 = pixel;
                        verticalLines[pixel].Stroke = lineStroke;
                    }
                }

                if (drawMode != 2)
                {
                    MainCanvas.Children.Clear();
                    foreach (Line line in verticalLines)
                    {
                        MainCanvas.Children.Add(line);
                    }
                }

                drawMode = 2;

                int samplesPerPixel = (int)(signal.NumberOfSamples / MainCanvas.Width) + 1;
                double sampleBatchMax = double.MinValue;
                double sampleBatchMin = double.MaxValue;
                
                for (int i = 1; i < signal.NumberOfSamples; i++)
                {
                    if (i % samplesPerPixel == 0)
                    {
                        verticalLines[i / samplesPerPixel].Y1 = middleY - sampleBatchMax * ratioY;
                        verticalLines[i / samplesPerPixel].Y2 = middleY - sampleBatchMin * ratioY;

                        sampleBatchMax = double.MinValue;
                        sampleBatchMin = double.MaxValue;
                    }

                    int sample = signal.GetSample(channelNumber);
                    if (sample > sampleBatchMax)
                    {
                        sampleBatchMax = sample;
                    }
                    if (sample < sampleBatchMin)
                    {
                        sampleBatchMin = sample;
                    }
                }
            }
        }
    }//class
}//ns
