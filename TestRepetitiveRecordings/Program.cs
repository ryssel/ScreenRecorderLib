﻿using System.Diagnostics;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace TestRepetitiveRecordings
{
    internal class Program
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public static bool KeepRecording { get; set; }
        public static IntPtr WindowHandle { get; set; }
        private static readonly string _applicationPath = @"mspaint.exe";
        //private static readonly string _applicationPath = @"C:\Enemies\Enemies.exe";
        private static Stopwatch _stopWatch;

        static void Main(string[] args)
        {
            var outputFolder = Directory.GetCurrentDirectory() + @"\Recordings";
            FileHelper.EnsureExists(outputFolder);

            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget()
            {
                FileName = Path.Combine(outputFolder, "log_${date:format=dd-MM-yyyy}.txt"),
                Name = "file",
                Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}",
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget, "*");
            LogManager.Configuration = config;

            Console.WriteLine("Press ENTER to start/stop recording or ESC to exit");
            _stopWatch = new Stopwatch();
            _stopWatch.Start();

            Task.Run(StartConcurrentRecordings);

            Task.Run(() =>
            {
                while (true)
                {
                    var state = KeepRecording ? "Recording" : "Stopped  ";
                    Console.Write($"\rElapsed: {_stopWatch.Elapsed:hh\\:mm\\:ss} - {state}");
                    Task.Delay(1000);
                }
            });
            while (true)
            {
                var info = Console.ReadKey(true);
                if (info.Key == ConsoleKey.Escape)
                {
                    break;
                }
                if (info.Key == ConsoleKey.Enter)
                {
                    KeepRecording = !KeepRecording;
                    if (KeepRecording)
                        _stopWatch.Start();
                    else
                    {
                        _stopWatch.Stop();
                    }
                }
            }

            KeepRecording = false;
        }

        private static async void StartConcurrentRecordings()
        {
            if (WindowHandle == IntPtr.Zero)
                StartNewProcess();
            KeepRecording = true;
            try
            {
                while (KeepRecording)
                {
                    var screenRecorderViewModel = new ScreenRecorderViewModel();
                    screenRecorderViewModel.Start(WindowHandle);
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    screenRecorderViewModel.Stop();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Exception thrown while running screenrecordings");
            }
        }

        private static void StartNewProcess()
        {
            var psi = new ProcessStartInfo(_applicationPath)
            {
                UseShellExecute = false,
            };
            var process = Process.Start(psi);
            do
            {
                process.Refresh();
            } while (process.MainWindowHandle == IntPtr.Zero);
            WindowHandle = process.MainWindowHandle;
        }

    }
}