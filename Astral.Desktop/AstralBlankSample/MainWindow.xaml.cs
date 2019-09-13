using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Astral.Device;
using Astral.Net;
using System.Drawing;
using AstralBlankSample.Utilities;
using AForgeFFT;
using System.Threading;

using Windows.Devices.Sensors;
using Windows.Foundation;
using System.Drawing.Imaging;
using System.IO;
using Astral.Messaging;
using Astral.Content;

namespace AstralBlankSample
{
    /*
     * Couple notes -
     * Run this system first (this will be the server), then run the Astral Mobile App
     * Update the Astral Mobile activity as you see fit and don't forget to update the IP Address
     */

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Instance Variables

        private NetworkManager networkManager = NetworkManager.Instance;
        private DeviceModel device;
        private Bitmap CurrentStamp;
       // private Windows.Devices.Sensors.Magnetometer magnetometer;
        private Windows.Devices.Sensors.Compass compass;

        int MicAttribute;

        WriteableBitmap writeableBmp;
        System.Windows.Point lastKnownCursorPosition;
        List<Utilities.Brush> Brushes;
        Utilities.Brush ActiveBrush;

        Random rnd;

        bool StampLoaded;
        long lastChange;
        long lastMicSent;
        long lastVibSent;
        //bool isTouchHeld;

        double lastFreq;
        double airbrushVolume;

        CompassReading tabletReading;
        double watchReading;
        double degreeDifference;
        double offset;
        double zTilt;
        

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            // Start the networking
            this.networkManager.Start();
            this.networkManager.DeviceAdded += OnDeviceConnected;

            rnd = new Random();

            MicAttribute = 0;

            writeableBmp = null;
            Brushes = new List<Utilities.Brush>();
            Brushes.Add(new Utilities.Brush());
            ActiveBrush = Brushes[0];
            lastKnownCursorPosition = new System.Windows.Point(0, 0);
            //isTouchHeld = false;
            StampLoaded = false;
            
            compass = Windows.Devices.Sensors.Compass.GetDefault();
            if (compass != null)
            {
                compass.ReportInterval = compass.MinimumReportInterval;
                compass.ReadingChanged += new TypedEventHandler<Windows.Devices.Sensors.Compass, CompassReadingChangedEventArgs>(ReadingChanged);
            }
            
            lastChange = 0;
            lastMicSent = 0;
            lastVibSent = 0;
            lastFreq = 0;

            airbrushVolume = 0;

            watchReading = 0;
            degreeDifference = 0;
            offset = 0;

            zTilt = 0;

            this.Loaded += OnLoaded;
            Canvas.TouchDown += Canvas_TouchDown;
            Canvas.TouchMove += Canvas_TouchMove;
            Canvas.TouchUp += Canvas_TouchUp;

        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            writeableBmp = BitmapFactory.New((int)this.ActualWidth, (int)this.ActualHeight);
            Canvas.Source = writeableBmp;
            using (writeableBmp.GetBitmapContext())
            {
                writeableBmp.Clear(Colors.White);
            }
            CurrentStamp = new Bitmap(320, 320, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        }
        #endregion

        #region Window Events

        private void Canvas_TouchUp(object sender, TouchEventArgs e)
        {
            var xPos = (int)e.GetTouchPoint(this).Position.X;
            var yPos = (int)e.GetTouchPoint(this).Position.Y;
            lastKnownCursorPosition = new System.Windows.Point(xPos, yPos);
        }

       

        private void Canvas_TouchDown(object sender, TouchEventArgs e)
        {
            var xPos = (int)e.GetTouchPoint(this).Position.X;
            var yPos = (int)e.GetTouchPoint(this).Position.Y;
            if (ActiveBrush.BrushType == Utilities.BrushTypes.STAMP)
            {
                if (!StampLoaded)
                {
                    Console.WriteLine("STAMP AT CENTER " + xPos + " " + yPos);
                    Console.WriteLine("DIMENSIONS: " + writeableBmp.PixelWidth + " " + writeableBmp.PixelHeight);
                    LoadStamp(ref xPos, ref yPos);
                    Bitmap ss = CurrentStamp.Clone() as Bitmap;
                    StampLoaded = true;

                }
                else
                {
                    Action DoStamp = () =>
                    {
                        for (int i = 0; i < 160; i++)
                        {
                            for (int j = 0; j < 160; j++)
                            {
                                if (xPos - 80 + i < 0 || yPos - 80 + j < 0 || xPos + i >= writeableBmp.PixelWidth || yPos + j >= writeableBmp.PixelHeight)
                                    continue;
                                else
                                    try
                                    {
                                        using (writeableBmp.GetBitmapContext())
                                        {
                                            System.Drawing.Color c_tmp = CurrentStamp.GetPixel(i*2, j*2);
                                            System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(c_tmp.A, c_tmp.R, c_tmp.G, c_tmp.B);
                                            writeableBmp.SetPixel(xPos - 80 + i, yPos - 80 + j, c);
                                        }

                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("Tried to set pixel at " + (xPos - 80 + i) + " " + (yPos - 80 + j));

                                    }
                            }
                        }
                    };
                    DoStamp.Invoke();
                }
            }
            lastKnownCursorPosition = new System.Windows.Point(xPos, yPos);
        }

        private async void ReadingChanged(object sender, CompassReadingChangedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() => {
                tabletReading = e.Reading;
            });
        }

        private void Canvas_TouchMove(object sender, TouchEventArgs e)
        {
            //Console.WriteLine((int)e.GetTouchPoint(this).Position.X + " " + (int)e.GetTouchPoint(this).Position.Y);
            //Console.WriteLine((int)e.GetTouchPoint(Canvas).Position.X + " " + (int)e.GetTouchPoint(Canvas).Position.Y);
            //Console.WriteLine((int)e.GetTouchPoint(null).Position.X + " " + (int)e.GetTouchPoint(null).Position.Y);
            

            using (writeableBmp.GetBitmapContext())
            {
                var xPos = (int)e.GetTouchPoint(this).Position.X;
                var yPos = (int)e.GetTouchPoint(this).Position.Y;
                if (Canvas.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    switch (ActiveBrush.BrushType)
                    {
                        case Utilities.BrushTypes.BRUSH:
                            //writeableBmp.DrawLineAa((int)lastKnownCursorPosition.X, (int)lastKnownCursorPosition.Y, xPos, yPos, ActiveBrush.Color, ActiveBrush.Radius * 2);

                            if (ActiveBrush.BrushShape == Utilities.BrushShapes.ELLIPSE)
                            {
                                writeableBmp.DrawLineAa((int)lastKnownCursorPosition.X, (int)lastKnownCursorPosition.Y, xPos, yPos, ActiveBrush.Color, ActiveBrush.Radius * 2);
                                writeableBmp.FillEllipseCentered(xPos, yPos, ActiveBrush.Radius, ActiveBrush.Radius, ActiveBrush.Color);
                                Console.WriteLine("Radius = " + ActiveBrush.Radius);
                            }

                            break;

                        case Utilities.BrushTypes.ERASER:
                            writeableBmp.DrawLineAa((int)lastKnownCursorPosition.X, (int)lastKnownCursorPosition.Y, xPos, yPos, System.Windows.Media.Colors.White, 50);
                            writeableBmp.FillEllipseCentered(xPos, yPos, 80, 80, Colors.White);
                            double velocity = Math.Sqrt((xPos - lastKnownCursorPosition.X) * (xPos - lastKnownCursorPosition.X) + (yPos - lastKnownCursorPosition.Y) * (yPos - lastKnownCursorPosition.Y))/5;
                            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                            if (velocity > 1 && now - lastVibSent > 100)
                            {
                                Console.WriteLine(velocity);
                                Message msg = new Message("Vibrate");
                                msg.AddField("Velocity", velocity);
                                device.Device.SendMessage(msg);
                                lastVibSent = now;
                            }
                            break;

                        case BrushTypes.AIRBRUSH:

                            this.Dispatcher.Invoke(new Action(delegate
                            {
                                
                                for (int i = 0; i < airbrushVolume; i++)
                                {
                                    double magnitude = 1;
                                    magnitude = zTilt < 5 ? 1 : Map(zTilt, 5, 9, 1, 10);

                                    System.Windows.Point p = RandomPointInCircle(50, xPos, yPos);
                                    p.Y += 50 * magnitude;
                                    double inRadians = degreeDifference * (Math.PI / 180);

                                    double c = Math.Cos(inRadians);
                                    double s = Math.Sin(inRadians);

                                    double tmpX = p.X - xPos;
                                    double tmpY = p.Y - yPos;

                                    double newX = tmpX * c - tmpY * s;
                                    double newY = tmpY * c + tmpX * s;

                                    p.X = newX + xPos;
                                    p.Y = newY + yPos;


                                    if (p.X >= 0 && p.Y >= 0 && p.X < writeableBmp.PixelWidth && p.Y < writeableBmp.PixelHeight)
                                        writeableBmp.SetPixel((int)p.X, (int)p.Y, ActiveBrush.Color);
                                    
                                }
                            }));
                            break;
                    }
                }
                lastKnownCursorPosition = new System.Windows.Point(xPos, yPos);
            }
        }
        #endregion
        

        #region Connection Initialization

        private void OnDeviceConnected(object sender, DeviceConnectedEventArgs e)
        {
            device = new DeviceModel(e.Device, e.Session);
            e.Device.MessageReceived += Device_MessageReceived;

            // Note: all networking events are running in a separate thread
            // you have to use this dispatcher invoke to do anything with the UI
            // otherwise you'll get a weird exception you can't make sense of
            Dispatcher.Invoke(new Action(delegate
            {

            }));

            InitializeDeviceEvents();
        }

        private void Device_MessageReceived(object sender, Message msg)
        {
            if (msg != null)
            {
                // get the message name
                string msgName = msg.Name;
                switch (msgName)
                {
                    case "AirflowChanged":
                        //Console.WriteLine("AIRFLOW CHANGED " + msg.GetFloatField("Amount"));
                        airbrushVolume = msg.GetFloatField("Amount");
                        //airbrushVolume = Math.Max(Math.Min(airbrushVolume, 200), 0);
                        if (airbrushVolume <= 10)
                            airbrushVolume = 0;
                        else if (airbrushVolume > 200)
                            airbrushVolume = 200;
                        else
                            airbrushVolume = Map(airbrushVolume, 10, 200, 20, 200);
                        
                        break;

                    case "SliderUpdate":
                        ActiveBrush.SetColor(msg.GetIntField("Hue"), msg.GetDoubleField("Val"), msg.GetDoubleField("Sat"), (int) msg.GetIntField("Size"));
                        break;
                    case "MicAttrChanged":
                        MicAttribute = msg.GetIntField("Mode");
                        break;

                    case "Recalibrate":
                        offset = 0;
                        break;
                    default:
                        break;

                }
            }
        }

        private void SendChangeTool(BrushTypes bt)
        {
            Message msg = new Message("ChangeTool");
            msg.AddField("Type", (int)bt);
            msg.AddField("MicMode", MicAttribute);
            msg.AddField("Hue", ActiveBrush.Hue);
            msg.AddField("Sat", ActiveBrush.Sat);
            msg.AddField("Val", ActiveBrush.Val);
            msg.AddField("Size", ActiveBrush.Radius);
            

            device.Device.SendMessage(msg);

            Console.WriteLine("Sent CHANGETOOL " + bt);
        }
        #endregion

        #region Device Events
        private void InitializeDeviceEvents()
        {
            device.Display.TouchDown += Display_TouchDown;
            device.Display.TouchUp += Display_TouchUp;
            //device.Accelerometer.AccelerationChanged += AccelerometerUpdated;
            device.Microphone.MicrophoneUpdated += OnMicrophoneUpdated;
            // newer version of the accelerometer
            device.AccelerationChanged += OnAccelerationChanged;
            device.Orientation.OrientationChanged += Orientation_OrientationChanged;
        }

        private void Orientation_OrientationChanged(object sender, AstralOrientationEventArgs e)
        {
            
            watchReading =  e.OrientationData.YawDegrees;
            //Console.WriteLine("tablt: " + tabletReading.HeadingMagneticNorth);
            //Console.WriteLine("watch: " + watchReading);

            if (watchReading < 0)
                watchReading =  360 + e.OrientationData.YawDegrees ;
            watchReading = watchReading % 360;
            degreeDifference = watchReading - tabletReading.HeadingMagneticNorth;


            if (degreeDifference > 180)
            {
                degreeDifference = degreeDifference - 360;
            }
            else if (degreeDifference < -180)
            {
                degreeDifference = degreeDifference + 360;
            }

            //Console.WriteLine(degreeDifference);
            
            

        }
        

        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (ActiveBrush.BrushType == BrushTypes.BRUSH && now - lastMicSent > 100) // if it's been 100 milliseconds since last mic message sent
            {
                new Thread(() =>
                {
                    short[] audioBuffer = e.MicrophoneData.Data;
                    double amplitude = Math.Max(audioBuffer.Max(), Math.Abs((int)audioBuffer.Min()));
                    double logLength = Math.Ceiling(Math.Log((double)audioBuffer.Length, 2.0));
                    int paddedLength = (int)Math.Pow(2.0, Math.Min(Math.Max(1.0, logLength), 14.0));
                    Complex[] audioBufferComplex = new Complex[paddedLength];
                    double[] magnitudes = new double[audioBuffer.Length / 2];

                    // Make Complex array for FFT
                    for (int i = 0; i < audioBuffer.Length; i++)
                    {
                        double hammingResult = (double)((0.53836 - (0.46164 * Math.Cos(Math.PI * 2 * (double)i / (double)(audioBuffer.Length - 1)))) * audioBuffer[i]);
                        audioBufferComplex[i] = new Complex(hammingResult, 0);
                    }

                    for (int i = audioBuffer.Length; i < paddedLength; i++)
                    {
                        audioBufferComplex[i] = new Complex(0, 0);
                    }
                    FourierTransform.FFT(audioBufferComplex, FourierTransform.Direction.Forward);
                    // calculate power spectrum
                    for (int i = 0; i < magnitudes.Length; i++)
                    {
                        magnitudes[i] = Math.Sqrt(audioBufferComplex[i].Re * audioBufferComplex[i].Re + audioBufferComplex[i].Im * audioBufferComplex[i].Im);
                    }
                    // Find largest peak
                    double max_magnitude = Double.NegativeInfinity;
                    int max_index = -1;
                    for (int i = 0; i < magnitudes.Length; i++)
                    {
                        if (magnitudes[i] > max_magnitude)
                        {
                            max_magnitude = magnitudes[i];
                            max_index = i;
                        }
                    }
                    // convert largest peak to frequency
                    lastFreq = max_index * 22050 / audioBuffer.Length;

                    if (amplitude > 1000)
                    {
                        
                        int freq = 0;
                        int prevhue = ActiveBrush.Hue;
                        double prevsat = ActiveBrush.Sat;
                        double prevval = ActiveBrush.Val;
                        int size = (int) Map(amplitude, 1000, 3000, 10, 50);
                        size = ClampInt(size, 10, 50);

                        freq = ClampInt((int) lastFreq, 1000, 3000);
                        Console.WriteLine("Before: " + freq);

                        switch (MicAttribute)
                        {
                            case 0: //None
                                ActiveBrush.SetColor(prevhue, prevval, prevsat, size);
                                break;
                            case 1: // Hue
                                freq = (int)Map(freq, 1000, 3000, 0, 360);
                                ActiveBrush.SetColor(freq, prevval, prevsat, size);
                                Console.WriteLine("Hue: " + ActiveBrush.Hue);
                                break;

                            case 2: // Sat
                                freq = (int) Map(freq, 1000, 3000, 0, 100);
                                ActiveBrush.SetColor(prevhue, prevval, (double)(freq/100.0f), size);
                                Console.WriteLine("Sat: " + ActiveBrush.Sat);
                                break;

                            case 3: // Val
                                freq = (int)Map(freq, 1000, 3000, 0, 100);
                                ActiveBrush.SetColor(prevhue, (double)(freq / 100.0f), prevsat, size);
                                Console.WriteLine("Val: " + ActiveBrush.Val);
                                break;

                            case 4: // Size
                                freq = (int)Map(freq, 1000, 3000, 0, 100);
                                ActiveBrush.SetColor(prevhue, prevval, prevsat, freq);
                                Console.WriteLine("Size: " + ActiveBrush.Radius);
                                break;
                            default:
                                ActiveBrush.SetColor(prevhue, prevval, prevsat, size);
                                break;
                        }
                        Console.WriteLine("After: "+ freq);

                        Message msg = new Message("BrushMic");
                        msg.AddField("Hue", ActiveBrush.Hue);
                        msg.AddField("Sat", ActiveBrush.Sat);
                        msg.AddField("Val", ActiveBrush.Val);
                        msg.AddField("Size", ActiveBrush.Radius);
                        device.Device.SendMessage(msg);
                    }
                }).Start();
                lastMicSent = now;
            }

        }

        private void Display_TouchDown(object sender, AstralTouchEventArgs e)
        {
            //isTouchHeld = true;
        }
        private void Display_TouchUp(object sender, AstralTouchEventArgs e)
        {
            //isTouchHeld = false;
        }

        private void OnAccelerationChanged(object sender, AccelerationDeviceModelEventArgs e)
        {
            zTilt = e.LinearZ;
            
            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - lastChange > 1000)
            {

                if (AccelIsBrush(e) && ActiveBrush.BrushType != Utilities.BrushTypes.BRUSH)
                {
                    ActiveBrush.BrushType = Utilities.BrushTypes.BRUSH;
                    SendChangeTool(Utilities.BrushTypes.BRUSH);
                    lastChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                }
                else if (AccelIsEraser(e) && ActiveBrush.BrushType != Utilities.BrushTypes.ERASER)
                {
                    ActiveBrush.BrushType = Utilities.BrushTypes.ERASER;
                    SendChangeTool(Utilities.BrushTypes.ERASER);
                    lastChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
                else if (AccelIsStamp(e) && ActiveBrush.BrushType != Utilities.BrushTypes.STAMP)
                {
                    CurrentStamp = new Bitmap(320, 320, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    StampLoaded = false;
                    ActiveBrush.BrushType = Utilities.BrushTypes.STAMP;
                    SendChangeTool(Utilities.BrushTypes.STAMP);
                    lastChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
                else if (AccelIsAirbrush(e) && ActiveBrush.BrushType != Utilities.BrushTypes.AIRBRUSH)
                {
                    ActiveBrush.BrushType = Utilities.BrushTypes.AIRBRUSH;
                    SendChangeTool(Utilities.BrushTypes.AIRBRUSH);
                    lastChange = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                }


            }

            // this event can get linear acceleration, acceleration and gravity values
            //Console.WriteLine( (int) e.LinearX + " :: " + (int) e.LinearY + " :: " + (int) e.LinearZ);
            //Console.WriteLine(e.LinearMagnitude);
        }
        #endregion

        #region Accel State Check
        private bool AccelIsStamp(AccelerationDeviceModelEventArgs e)
        {
            if ((int)e.LinearZ == 0 && e.LinearY > 7)
            {
                return true;
            }

            return false;
        }

        private bool AccelIsBrush(AccelerationDeviceModelEventArgs e)
        {
            if (e.LinearY > 0 && e.LinearY < 9.81)
            {
                if (e.LinearZ > 3 && e.LinearZ < 9.81)
                {
                    return true;
                }
            }
            return false;
        }

        private bool AccelIsEraser(AccelerationDeviceModelEventArgs e)
        {
            if (e.LinearY > 0 && e.LinearY < 9.81)
            {
                if (e.LinearZ < -3 && e.LinearZ > -9.81)
                {
                    return true;
                }
            }
            return false;
        }

        private bool AccelIsAirbrush(AccelerationDeviceModelEventArgs e)
        {
            if (e.LinearY < 0 && e.LinearY > -9.81)
            {
                if (e.LinearZ > 3 && e.LinearZ < 9.81)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Helpers

        public double Map(double value, double from1, double to1, double from2, double to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public int ClampInt(int val, int min, int max)
        {
            if (val < min)
                return min;
            else if (val > max)
                return max;

            return val;
        }
        

        private void LoadStamp(ref int xPos, ref int yPos)
        {
            using (Graphics g = Graphics.FromImage(CurrentStamp))
            {
                g.CopyFromScreen(xPos *2- 160, yPos*2- 160,
                    0, 0, new System.Drawing.Size(320, 320),
                    CopyPixelOperation.SourceCopy);
            }

            Dispatcher.Invoke(() => {
                
                MemoryStream strm = new MemoryStream();

                Bitmap tmp = (Bitmap) CurrentStamp.Clone();

                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                tmp.Save(strm, jpgEncoder, myEncoderParameters);

                byte[] t = strm.GetBuffer();

                Message message = new Message("Stamp");
                message.AddField("buffer", t);
                device.Device.SendMessage(message);

            });
            //Console.WriteLine("STAMP LOADED FROM CENTER " + xPos + " " + yPos);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private System.Windows.Point RandomPointInCircle(double r, double x, double y)
        {
            double magnitude = 1;
            magnitude = zTilt < 5 ? 1 : Map(zTilt, 5, 9, 1, 10);

            double a = rnd.NextDouble();
            double b = rnd.NextDouble();
            if (b < a)
            {
                double temp = b;
                b = a;
                a = temp;
            }
            double xOffset = r * (b * Math.Cos(2 * Math.PI * a / b));
            double yOffset = r * (b * Math.Sin(2 * Math.PI * a / b));

            return new System.Windows.Point(x + xOffset, y + yOffset * magnitude);
        }
    }
    #endregion
}
