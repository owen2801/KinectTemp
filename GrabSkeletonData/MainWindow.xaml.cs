using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using System.IO;

namespace GrabSkeletonData
{

    public partial class MainWindow : Window
    {
        private KinectSensor sensor;
        Stream recordStream;
        static KinectRecorder recorder;

        public MainWindow()
        {
            InitializeComponent();
            KinectSensor.KinectSensors.StatusChanged += KinectSensors_StatusChanged;
            //this.KinectDevice = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);
            sensor = KinectSensor.KinectSensors[0];
            this.sensor.SkeletonStream.Enable();
            this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            this.sensor.Start();
        }
         
        public KinectSensor KinectDevice
        {
            get { return this.sensor; }
            set
            {
                if (this.sensor != value)
                {
                    //Uninitialize
                    if (this.sensor != null)
                    {
                        this.sensor.Stop();
                        this.sensor.ColorStream.Disable();
                        this.sensor.SkeletonStream.Disable();
                    }

                    this.sensor = value;

                    //Initialize
                    if (this.sensor != null)
                    {
                        if (this.sensor.Status == KinectStatus.Connected)
                        {
                            this.sensor.SkeletonStream.Enable();
                            this.sensor.ColorStream.Enable();
                            this.sensor.Start();
                        }
                    }
                }
            }
        }

        private void KinectSensors_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Initializing:
                case KinectStatus.Connected:
                case KinectStatus.NotPowered:
                case KinectStatus.NotReady:
                case KinectStatus.DeviceNotGenuine:
                    this.KinectDevice = e.Sensor;
                    break;
                case KinectStatus.Disconnected:
                    //TODO: Give the user feedback to plug-in a Kinect device.    
                    this.AddChild(MessageBox.Show("Disconnected"));

                    this.KinectDevice = null;
                    break;
                default:
                    //TODO: Show an error state
                    break;
            }
        }

        private void recordOption_Start(object sender, RoutedEventArgs e)
        {
            
            recordStream = File.Create(@"C:\test.dat");            
            recorder = new KinectRecorder(KinectRecordOptions.Skeletons | KinectRecordOptions.Color, recordStream);
            //sensor.ColorFrameReady += new EventHandler<ColorImageFrameReadyEventArgs>(sensor_ColorImageFrameReady);
            sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonImageFrameReady);
        }

        void StopRecord()
        {
            if (recorder != null)
            {
                recorder.Stop();
                recorder = null;
                return;
            }            
        }

        /*
        static void sensor_ColorImageFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var cframe = e.OpenColorImageFrame())
            {
                if (cframe == null | recorder == null)
                    return;
                recorder.Record( cframe);
            }
        }
         * */

        static void sensor_SkeletonImageFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (var sframe = e.OpenSkeletonFrame())
            {
                if (sframe == null)
                    return;
                recorder.Record(sframe);
            }
        }

        private void record_stop_Click(object sender, RoutedEventArgs e)
        {

            StopRecord();
        }

        

    }
}
