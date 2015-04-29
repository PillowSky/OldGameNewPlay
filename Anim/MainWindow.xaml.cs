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
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using Microsoft.Kinect;
using LightBuzz.Vitruvius;
using LightBuzz.Vitruvius.WPF;

namespace Anim {
    public partial class MainWindow : Window {
        const int GAME_WIDTH = 800;
        const int GAME_HEIGHT = 600;
        const int CLICK_THRESHOLD = 2000;

        private KinectSensor sensor;
        private bool gaming = false;
        private int lastX = 0;
        private int lastY = 0;

        public MainWindow() {
            InitializeComponent();
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
                MessageBox.Show("未发现Kinect，重连一下试试？", "未发现Kinect", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void onTipButtonClick(object sender, MouseButtonEventArgs e) {
            TipWindow tip = new TipWindow();
            tip.ShowDialog();
        }

        private void onGoButtonClick(object sender, MouseButtonEventArgs e) {
            if (!gaming) {
                try {
                    Process.Start("FeedingFrenzyTwo.exe");
                    gaming = true;
                } catch (Win32Exception) {
                    MessageBox.Show("未发现游戏主程序，检查一下路径？", "未发现游戏主程序", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
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
                    var skeleton = skeletonFrame.Skeletons().Where(s => s.TrackingState == SkeletonTrackingState.Tracked).OrderBy(s => s.Joints[JointType.Head].Position.Z).FirstOrDefault();
                    if (skeleton != null) {
                        if (!gaming) {
                            canvas.ClearSkeletons();
                            canvas.DrawSkeleton(skeleton);
                        }
                        extractEvent(skeleton);
                    }
                }
            }
        }

        private void extractEvent(Skeleton skeleton) {
            int rightX = (int)(skeleton.Joints[JointType.HandRight].Position.X * GAME_WIDTH + GAME_WIDTH / 2);
            int rightY = (int)(skeleton.Joints[JointType.HandRight].Position.Y * -GAME_HEIGHT + GAME_HEIGHT / 2);
            int leftX = (int)(skeleton.Joints[JointType.HandLeft].Position.X * GAME_WIDTH + GAME_WIDTH);
            int leftY = (int)(skeleton.Joints[JointType.HandLeft].Position.Y * -GAME_HEIGHT + GAME_HEIGHT / 2);

            saturate(rightX, 0, GAME_WIDTH);
            saturate(rightY, 0, GAME_HEIGHT);
            saturate(leftX, 0, GAME_WIDTH);
            saturate(leftY, 0, GAME_HEIGHT);

            SetCursorPos(rightX, rightY);

            double dis = Math.Pow(leftX - lastX, 2) + Math.Pow(leftY - lastY, 2);
            if (dis > CLICK_THRESHOLD) {
                mouse_event(MOUSEEVENTF_LEFTDOWN + MOUSEEVENTF_ABSOLUTE, rightX, rightY, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, rightX, rightY, 0, 0);
            }
            lastX = leftX;
            lastY = leftY;
        }

        private int saturate(int num, int min, int max) {
            if (num < min) {
                return min;
            } else {
                if (num > max) {
                    return max;
                } else {
                    return num;
                }
            }
        }

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButton, int dwExtraInfo);

        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        const int MOUSEEVENTF_RIGHTUP = 0x0010;
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        const int MOUSEEVENTF_MIDDLEUP = 0x0040;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;
    }
}
