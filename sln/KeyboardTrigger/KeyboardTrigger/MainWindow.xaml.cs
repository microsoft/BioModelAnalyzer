using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace KeyboardTrigger
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private bool isListening = false;
        private TimeSpan lastTime = TimeSpan.FromTicks(Stopwatch.GetTimestamp());
        private double currentDif = 10;
        private Random r = new Random();

        public MainWindow()
        {
            InitializeComponent();
            timer.Tick += timer_Tick;

            WinKeyCapture.Key7IsPressed += (o, e) => {
                isListening = !isListening;

                if (isListening)
                {
                    lastTime = TimeSpan.FromTicks(Stopwatch.GetTimestamp());
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                }
            };

            WinKeyCapture.SetHook();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            var current = TimeSpan.FromTicks(Stopwatch.GetTimestamp());
            var elapsed = current - lastTime;
            if (elapsed.TotalMilliseconds >= currentDif)
            {
                Console.WriteLine("Past: " + elapsed.TotalMilliseconds + " Diff: " + currentDif);
                currentDif = r.NextDouble() * TimeSpan.FromSeconds(1).TotalMilliseconds / 4 + TimeSpan.FromSeconds(1).TotalMilliseconds / 8;
                lastTime = current;
                Press();
            }
        }
        
        const int VK_UP = 0x26; //up key
        const int VK_Q = 0x113;  //down key
        const int VK_W = 0x87;
        const int VK_E = 0x69;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private int Press()
        {
            //Press the key
            var w = KeyInterop.VirtualKeyFromKey(Key.W);
            keybd_event((byte)w, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            Thread.Sleep(5);

            var q = KeyInterop.VirtualKeyFromKey(Key.Q);
            //keybd_event((byte)q, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            //Thread.Sleep(5);
            
            var e = KeyInterop.VirtualKeyFromKey(Key.E);
            keybd_event((byte)e, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            return 0;
        }

        //81 87 69

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
    }

    
}

