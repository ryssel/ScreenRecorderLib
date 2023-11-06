using System.Diagnostics;
using ScreenRecorderLib;

namespace TestRepetitiveRecordings
{
    public class ScreenRecorder
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private Recorder _rec;
        private string _videoPath;
        private static int _instance;
        public event EventHandler<RecordingCompleteEventArgs> OnRecordingComplete;

        private RecorderOptions RecorderOptions { get; set; }

        public H264Profile EncoderProfile { get; set; }

        public H264BitrateControlMode BitrateMode { get; set; }

        public int Framerate { get; set; }

        public int Bitrate { get; set; }

        public int Quality { get; set; }

        public bool IsMousePointerEnabled { get; set; }

        public ScreenRecorder()
        {
            Log.Info($"ScreenRecorder #{++_instance} created");
        }

        public void CreateRecording(string videoPath, IntPtr windowHandle, string logFileName)
        {
            if (logFileName == null)
            {
                logFileName = "ScreenRecorder.log";
                if (File.Exists(logFileName) && File.GetCreationTime(logFileName) < DateTime.Today)
                    PathHelper.LogRotate(logFileName, 10);
            }

            RecorderOptions = RecorderOptions.Default;
            RecorderOptions.LogOptions = new LogOptions
            {
                //This enabled logging in release builds.
                IsLogEnabled = true,
                //If this path is configured, logs are redirected to this file.
                LogFilePath = logFileName,

                LogSeverityLevel = ScreenRecorderLib.LogLevel.Debug
            };
            RecorderOptions.VideoEncoderOptions = new VideoEncoderOptions
            {
                //Currently supported are H264VideoEncoder and H265VideoEncoder
                Encoder = new H264VideoEncoder
                {
                    BitrateMode = BitrateMode,
                    EncoderProfile = EncoderProfile,
                },
                Framerate = Framerate,
                Bitrate = Bitrate,
                Quality = Quality,
                //Fragmented Mp4 allows playback to start at arbitrary positions inside a video stream,
                //instead of requiring to read the headers at the start of the stream.
                IsFragmentedMp4Enabled = true,
                //If throttling is disabled, out of memory exceptions may eventually crash the program,
                //depending on encoder settings and system specifications.
                IsThrottlingDisabled = false,
                //Hardware encoding is enabled by default.
                IsHardwareEncodingEnabled = true,
                //Low latency mode provides faster encoding, but can reduce quality.
                IsLowLatencyEnabled = false,
                //Fast start writes the mp4 header at the beginning of the file, to facilitate streaming.
                IsMp4FastStartEnabled = false
            };

            // Don't record audio until we know what to use it for
            RecorderOptions.AudioOptions.IsAudioEnabled = false;

            // According to this issue RecorderOptions.MouseOptions does not work use windowRecordingSource.IsCursorCaptureEnabled instead
            // https://github.com/sskodje/ScreenRecorderLib/issues/253
            // RecorderOptions.MouseOptions = new MouseOptions { IsMousePointerEnabled = IsMousePointerEnabled };


            Log.Info($"ScreenRecorder #{_instance} ScreenRecorder: CreateRecording {_videoPath}");
            Log.Info($"ScreenRecorder #{_instance} EncoderProfile = {EncoderProfile} ");
            Log.Info($"ScreenRecorder #{_instance} BitrateMode = {BitrateMode}");
            Log.Info($"ScreenRecorder #{_instance} Framerate = {Framerate} FPS");
            Log.Info($"ScreenRecorder #{_instance} Bitrate = {Bitrate} BPS");
            Log.Info($"ScreenRecorder #{_instance} Quality = {Quality}");
            _videoPath = videoPath;
            var windowRecordingSource = new WindowRecordingSource(windowHandle);
            // According to this issue RecorderOptions.MouseOptions does not work use windowRecordingSource.IsCursorCaptureEnabled instead
            // https://github.com/sskodje/ScreenRecorderLib/issues/253
            windowRecordingSource.IsCursorCaptureEnabled = IsMousePointerEnabled;
            //windowRecordingSource.OutputSize = new ScreenSize(800, 600);

            RecorderOptions.SourceOptions.RecordingSources.Add(windowRecordingSource);

            _rec = Recorder.CreateRecorder(RecorderOptions);
            // According to this issue RecorderOptions.MouseOptions does not work use windowRecordingSource.IsCursorCaptureEnabled instead
            // https://github.com/sskodje/ScreenRecorderLib/issues/253
            //_rec?.GetDynamicOptionsBuilder()
            //    .SetDynamicMouseOptions(new DynamicMouseOptions { IsMousePointerEnabled = RecorderOptions.MouseOptions.IsMousePointerEnabled })
            //    .Apply();

            _rec.OnRecordingComplete += Rec_OnRecordingComplete;
            _rec.OnRecordingFailed += Rec_OnRecordingFailed;
            _rec.OnStatusChanged += Rec_OnStatusChanged;
            //_rec.OnFrameRecorded += _rec_OnFrameRecorded;
            _rec.Record(_videoPath);
        }

        //private void _rec_OnFrameRecorded(object sender, FrameRecordedEventArgs e)
        //{
        //    //Log.Info($"ScreenRecorder: OnFrameRecorded");
        //}

        public void EndRecording()
        {
            Log.Info($"ScreenRecorder #{_instance}: EndRecording {_videoPath}");
            if (_rec == null)
            {
                Log.Error(new Exception($"ScreenRecorder EndRecording() called but _rec was null Instance # = {_instance}"));
            }
            else
            {
                _rec.Stop();
            }
        }

        public void Play()
        {
            Log.Info($"ScreenRecorder #{_instance}: Play  {_videoPath}");
            Process.Start(new ProcessStartInfo(_videoPath) { UseShellExecute = true });
        }

        private void Rec_OnRecordingComplete(object sender, RecordingCompleteEventArgs e)
        {
            Log.Info($"ScreenRecorder#{_instance}: Rec_OnRecordingComplete  {_videoPath}");
            Task.Run(() =>
            {
                CleanupResources();
                OnRecordingComplete?.Invoke(null, e);
            });
        }

        private void Rec_OnRecordingFailed(object sender, RecordingFailedEventArgs e)
        {
            Log.Info($"ScreenRecorder#{_instance}: Rec_OnRecordingFailed Error: {e.Error} {_videoPath}");
            Task.Run(() =>
            {
                Log.Error($"ScreenRecorder #{_instance} failed: {e.Error}");
                Log.Error(new Exception($"ScreenRecorder failed: {e.Error} Instance # = {_instance}"));
                CleanupResources();
            });
        }

        private void CleanupResources()
        {
            _rec?.Dispose();
            _rec = null;
        }


        private void Rec_OnStatusChanged(object sender, RecordingStatusEventArgs e)
        {
            Task.Run(() =>
            {
                RecorderStatus status = e.Status;
                Log.Info($"ScreenRecorder: #{_instance}: Rec_OnStatusChanged Status = {e.Status}");
            });
        }
    }
}
