using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Astral;
using Astral.Device;
using Astral.Net;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using Astral.Messaging;

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
        WriteableBitmap writeableBmp;
        System.Windows.Point lastKnownCursorPosition;
        List<Utilities.Brush> Brushes;
        Utilities.Brush ActiveBrush;
        private Bitmap CurrentStamp;
        bool StampLoaded;
        

        bool isTouchHeld;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            // Start the networking
            this.networkManager.Start();
            this.networkManager.DeviceAdded += OnDeviceConnected;


            writeableBmp = null;
            Brushes = new List<Utilities.Brush>();
            Brushes.Add(new Utilities.Brush());
            ActiveBrush = Brushes[0];
            lastKnownCursorPosition = new System.Windows.Point(0, 0);
            isTouchHeld = false;
            StampLoaded = false;
            

            this.Loaded += OnLoaded;
            Canvas.TouchMove += Canvas_TouchMove;
            Canvas.MouseDown += Canvas_MouseDown;
            Canvas.MouseMove += Canvas_MouseMove;
            
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            writeableBmp = BitmapFactory.New((int)this.ActualWidth, (int)this.ActualHeight);
            //Console.WriteLine(writeableBmp.PixelWidth + " " + writeableBmp.PixelHeight);
            Canvas.Source = writeableBmp;
            using (writeableBmp.GetBitmapContext())
            {
                writeableBmp.Clear(Colors.White);
            }
            CurrentStamp = new Bitmap(
                320, 320, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        }

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            var xPos = (int)e.GetPosition(this).X;
            var yPos = (int)e.GetPosition(this).Y;
            if (ActiveBrush.BrushType == Utilities.BrushTypes.STAMP)
            {
                if (!StampLoaded)
                {
                    LoadStamp(ref xPos, ref yPos);

                    Bitmap ss = CurrentStamp.Clone() as Bitmap;

                    Console.WriteLine("Take a screenshot");

                    MemoryStream strm = new MemoryStream();

                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

                    EncoderParameters myEncoderParameters = new EncoderParameters(1);

                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    ss.Save(strm, jpgEncoder, myEncoderParameters);
                    
                    Astral.Content.Screenshot screenshot = new Astral.Content.Screenshot(strm.GetBuffer());
                    Message screenshotMsg = ScreenshotMessage.CreateInstance(screenshot);
                    device.Device.SendMessage(screenshotMsg);
                    
                    strm.Dispose();
                    
                    StampLoaded = true;

                }
                else {
                    Action DoStamp = () =>
                    {
                        for (int i = 0; i < 320; i++)
                        {
                            for (int j = 0; j < 320; j++)
                            {
                                if (xPos - 160 + i < 0 || yPos - 160 + j < 0 || xPos + i >= writeableBmp.Width || yPos + j >= writeableBmp.Height)
                                    continue;
                                else
                                    try
                                    {
                                        using (writeableBmp.GetBitmapContext())
                                        {
                                            System.Drawing.Color c_tmp = CurrentStamp.GetPixel(i, j);
                                            System.Windows.Media.Color c = System.Windows.Media.Color.FromArgb(c_tmp.A, c_tmp.R, c_tmp.G, c_tmp.B);
                                            writeableBmp.SetPixel(xPos - 160 + i, yPos - 160 + j, c);
                                        }

                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("Tried to set pixel at " + (xPos - 160 + i) + " " + (yPos - 160 + j));
                                        
                                    }
                            }
                        }
                    };
                    DoStamp.Invoke();
                }
            }
        }

        private void LoadStamp(ref int xPos, ref int yPos)
        {
            using (Graphics g = Graphics.FromImage(CurrentStamp))
            {
                if (xPos - 160 < 0)
                    xPos = 160;
                if (yPos - 160 < 0)
                    yPos = 160;

                g.CopyFromScreen(xPos - 160, yPos - 160,
                    0, 0, new System.Drawing.Size(320, 320),
                    CopyPixelOperation.SourceCopy);
            }
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

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            using (writeableBmp.GetBitmapContext())
            {
                var xPos = (int)e.GetPosition(this).X;
                var yPos = (int)e.GetPosition(this).Y;
                if (Canvas.IsMouseOver && Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    switch (ActiveBrush.BrushType)
                    {
                        case Utilities.BrushTypes.BRUSH:
                            //writeableBmp.DrawLineAa((int)lastKnownCursorPosition.X, (int)lastKnownCursorPosition.Y, xPos, yPos, ActiveBrush.Color, ActiveBrush.Radius * 2);

                            if (ActiveBrush.BrushShape == Utilities.BrushShapes.ELLIPSE)
                                writeableBmp.FillEllipseCentered(xPos, yPos, ActiveBrush.Radius, ActiveBrush.Radius, ActiveBrush.Color);

                            else if (ActiveBrush.BrushShape == Utilities.BrushShapes.SQUARE)
                                writeableBmp.FillRectangle(xPos - ActiveBrush.Radius, yPos - ActiveBrush.Radius, xPos + ActiveBrush.Radius, yPos + ActiveBrush.Radius, ActiveBrush.Color);
                            
                            break;

                        case Utilities.BrushTypes.ERASER:
                            //writeableBmp.DrawLineAa((int)lastKnownCursorPosition.X, (int)lastKnownCursorPosition.Y, xPos, yPos, Colors.White, ActiveBrush.Radius * 2);
                            writeableBmp.FillEllipseCentered(xPos, yPos, ActiveBrush.Radius, ActiveBrush.Radius, Colors.White);
                            break;

                        case Utilities.BrushTypes.STAMP:
                            
                            break;
                    }
                }
                lastKnownCursorPosition = new System.Windows.Point(xPos, yPos);
            }
        }

        private void Canvas_TouchMove(object sender, TouchEventArgs e)
        {
            using (writeableBmp.GetBitmapContext())
            {
                var xPos = (int) e.GetTouchPoint(Canvas).Position.X;
                var yPos = (int) e.GetTouchPoint(Canvas).Position.Y;
                //Console.WriteLine(xPos + " " + yPos);

                if (Canvas.IsMouseOver)
                {
                    if (ActiveBrush.BrushType == Utilities.BrushTypes.BRUSH)
                    {
                        writeableBmp.SetPixel(xPos, yPos, ActiveBrush.Color);
                    }
                    else if (ActiveBrush.BrushType == Utilities.BrushTypes.ERASER)
                    {
                        writeableBmp.SetPixel(xPos, yPos, Colors.Transparent);
                    }
                }
            }
        }

        #endregion

        #region Connection Initialization

        private void OnDeviceConnected(object sender, DeviceConnectedEventArgs e)
        {
            device = new DeviceModel(e.Device, e.Session);

            // Note: all networking events are running in a separate thread
            // you have to use this dispatcher invoke to do anything with the UI
            // otherwise you'll get a weird exception you can't make sense of
            Dispatcher.Invoke(new Action(delegate
            {
                
            }));
            
            InitializeDeviceEvents();
        }

        private void InitializeDeviceEvents()
        {
            device.Display.TouchDown += OnTouchDown;
            device.Display.TouchUp += Display_TouchUp;
            //device.Accelerometer.AccelerationChanged += AccelerometerUpdated;
            device.Microphone.MicrophoneUpdated += OnMicrophoneUpdated;
            device.Magnetometer.MagnetometerChanged += OnMagnetometerChanged;
            device.Compass.HeadingChanged += HeadingChanged;
            // newer version of the accelerometer
            device.AccelerationChanged += OnAccelerationChanged;
        }


        private void HeadingChanged(object sender, AstralCompassEventArgs e)
        {
            Console.WriteLine(e.CompassData.Heading);
        }

        private void OnMagnetometerChanged(object sender, AstralMagnetometerEventArgs e)
        {
            //Console.WriteLine(e.MagnetometerData.X + " " + e.MagnetometerData.Y + " " + e.MagnetometerData.Z);
        }

        #endregion

        //private void AccelerometerUpdated(object sender, AstralAccelerometerEventArgs e)
        //{
            //Console.WriteLine(e.AccelerationData.X + " :: " + e.AccelerationData.Y + " :: " + e.AccelerationData.Z);
        //}

        private void OnMicrophoneUpdated(object sender, AstralMicrophoneEventArgs e)
        {
            //Console.WriteLine(e.MicrophoneData.Amplitude);
            // full array of values
            //Console.WriteLine(e.MicrophoneData.Data);
        }

        private void OnTouchDown(object sender, AstralTouchEventArgs e)
        {
            isTouchHeld = true;

            if (ActiveBrush.BrushType == Utilities.BrushTypes.STAMP)
            {

            }

            /* Vibrate and SendMessage test
            Message msg = new Message("Vibrate");
            msg.AddField("Milliseconds", (long) 50);
            msg.AddField("Amplitude", (int) 100);
            device.Device.SendMessage(msg);
            */
        }
        private void Display_TouchUp(object sender, AstralTouchEventArgs e)
        {
            isTouchHeld = false;
        }

        private void OnAccelerationChanged(object sender, AccelerationDeviceModelEventArgs e)
        {
            if(isTouchHeld)
            {
                if ((int) e.LinearX == 0 && ActiveBrush.BrushType != Utilities.BrushTypes.BRUSH)
                {
                    ActiveBrush.BrushType = Utilities.BrushTypes.BRUSH; 
                    SendVibrate(50, 100);
                } 
                else if ((int)e.LinearX == 4 && ActiveBrush.BrushType != Utilities.BrushTypes.ERASER)
                {
                    ActiveBrush.BrushType = Utilities.BrushTypes.ERASER;
                    SendVibrate(50, 100);
                    SendVibrate(50, 100);
                }
                else if ((int)e.LinearX == -4 && ActiveBrush.BrushType != Utilities.BrushTypes.STAMP)
                {
                    ActiveBrush.BrushType = Utilities.BrushTypes.STAMP;
                    
                    SendVibrate(50, 100);
                    SendVibrate(50, 100);
                }
            }

            //if ((int) e.LinearY < 0)


            // this event can get linear acceleration, acceleration and gravity values
            //Console.WriteLine( (int) e.LinearX + " :: " + (int) e.LinearY + " :: " + (int) e.LinearZ);
            //Console.WriteLine(e.LinearMagnitude);
        }

        private void SendVibrate(long ms, int amp)
        {
            Message msg = new Message("Vibrate");
            msg.AddField("Milliseconds", ms);
            msg.AddField("Amplitude", amp);
            device.Device.SendMessage(msg);
        }
    }
}
