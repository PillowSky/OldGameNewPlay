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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.WPF;

namespace Anim {
    public partial class MainWindow : Window {
        private KinectSensor sensor;
        private bool gaming = false;
        public MainWindow() {
            InitializeComponent();
        }

        private void onTipButtonClick(object sender, MouseButtonEventArgs e) {
            TipWindow tip = new TipWindow();
            tip.Show();
        }

        private void onLoad(object sender, RoutedEventArgs e) {
            sensor = SensorExtensions.Default();
            if (sensor != null) {
                sensor.ColorStream.Enable();
                sensor.ColorFrameReady += onColorFrameReady;
                sensor.SkeletonStream.Enable();
                sensor.SkeletonFrameReady += onSkeletonFrameReady;
                sensor.Start();
            } else {
                MessageBox.Show("未发现Kinect，插拔一下试试？", "未发现Kinect", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void onColorFrameReady(object sender, ColorImageFrameReadyEventArgs e) {
            using (var colorFrame = e.OpenColorImageFrame()) {
                if (colorFrame != null) {
                    camera.Source = colorFrame.ToBitmap();
                }
            }
        }

        private void onSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e) {
            using (var skeletonFrame = e.OpenSkeletonFrame()) {
                if (skeletonFrame != null) {
                    canvas.ClearSkeletons();
                    Skeleton[] allSkeletons = new Skeleton[6];
                    skeletonFrame.CopySkeletonDataTo(allSkeletons);

                    var skeleton = skeletonFrame.Skeletons().Where(s => s.TrackingState == SkeletonTrackingState.Tracked).OrderBy(s => s.Joints[JointType.Head].Position.Z).FirstOrDefault();
                    if (skeleton != null) {
                        if (!gaming) {
                            canvas.DrawSkeleton(skeleton);
                        }
                    }
                }
            }
        }
    }
}
