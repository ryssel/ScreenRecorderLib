using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using ScreenRecorderLib;

namespace TestRepetitiveRecordings
{
    public class ScreenRecorderViewModel
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private string _videoPath;
        private DateTimeOffset _startTime;
        private DateTimeOffset _endTime;
        private static int _instance;
        private ScreenRecorder _screenRecorder;

        public ScreenRecorderViewModel()
        {
            Log.Info($"ScreenRecorderViewModel #{++_instance} created");
        }

        public List<DateTimeOffset> ImpactTimes { get; set; }


        public static string ToValidFilename(string filename)
        {
            // Replace invalid characters with "_" char.
            return Regex.Replace(filename, @"[^\w\.-]", "_");
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindow(IntPtr hWnd);

        public void Start(IntPtr windowHandle)
        {
            _screenRecorder = new ScreenRecorder
            {
                BitrateMode = H264BitrateControlMode.CBR,
                EncoderProfile = H264Profile.Main,
                Framerate = 30,
                Bitrate = 8000 * 1000,
                Quality = 70,
                IsMousePointerEnabled = false
            };

            var outputFolder = Directory.GetCurrentDirectory() + @"\Recordings";
            FileHelper.EnsureExists(outputFolder);
            _startTime = DateTime.Now;
            var fileName = Path.Combine(outputFolder, @"ScreenRecording_" + ToValidFilename(_startTime.ToString("s", CultureInfo.InvariantCulture)));
            _videoPath = fileName + ".mp4";
            var logFileName = fileName + ".log";

            _screenRecorder.CreateRecording(_videoPath, windowHandle, logFileName);
            _screenRecorder.OnRecordingComplete += _screenRecorder_OnRecordingComplete;
        }

        public void Stop()
        {
            _screenRecorder.EndRecording();
        }

        private async void _screenRecorder_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            _screenRecorder.OnRecordingComplete -= _screenRecorder_OnRecordingComplete;

            _endTime = DateTime.Now;

            Log.Info($"ScreenRecorderViewModel  #{_instance} OnRecordingComplete.");
        }

        public void Play()
        {
            _screenRecorder.Play();
        }
    }
}
