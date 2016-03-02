using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Net;
using System.Timers;
using System.Threading;
using System.ComponentModel;

namespace SLSpotifyCon
{
    class Program
    {
        private static System.Timers.Timer T = new System.Timers.Timer();
        private static int i = 0;
        private static String curr = "";
        private static bool firstRun = true;

        private static readonly HttpListener listener = new HttpListener();

        public static string frames(int index)
        {
            if (index < 0 || index > 5) { index = 0; }
            List<string> frames = new List<string>();
            frames.Add("█▒▒▒▒██▒▒▒▒█\n▒█▒▒█▒▒█▒▒█▒ Listening for connections... ^_^\n▒▒██▒▒▒▒██▒▒");
            frames.Add("▒▒▒▒██▒▒▒▒██\n█▒▒█▒▒█▒▒█▒▒ Listening for connections... ^_^\n▒██▒▒▒▒██▒▒▒");
            frames.Add("▒▒▒██▒▒▒▒██▒\n▒▒█▒▒█▒▒█▒▒█ Listening for connections... ^_^\n██▒▒▒▒██▒▒▒▒");
            frames.Add("▒▒██▒▒▒▒██▒▒\n▒█▒▒█▒▒█▒▒█▒ Listening for connections... ^_^;\n█▒▒▒▒██▒▒▒▒█");
            frames.Add("▒██▒▒▒▒██▒▒▒\n█▒▒█▒▒█▒▒█▒▒ Listening for connections... ^_^;\n▒▒▒▒██▒▒▒▒██");
            frames.Add("██▒▒▒▒██▒▒▒▒\n▒▒█▒▒█▒▒█▒▒█ Listening for connections... ^_^;\n▒▒▒██▒▒▒▒██▒");
            return frames[index];
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(frames(i) + "\n" + curr);
            if (i > 5) { i = 0; }
            i++;
            GC.Collect();
        }

        private static void start()
        {
            if (firstRun)
            {
                listener.Prefixes.Add("X");
            }
            firstRun = false;
            listener.Start();
            listen();
        }

        private static void listen()
        {
            IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
            result.AsyncWaitHandle.WaitOne();
        }

        private static void ListenerCallback(IAsyncResult ar)
        {
            HttpListenerContext context = listener.EndGetContext(ar);
            HttpListenerRequest request = context.Request;
            try
            {
                HttpListenerResponse response = context.Response;

                response.Headers.Add(HttpResponseHeader.Server, "Apache/2.2.22 (FreeBSD)");
                response.Headers.Add(HttpResponseHeader.ContentType, "text/html; charset=utf-8");

                Process[] p = Process.GetProcessesByName("spotify");
                String nowPlaying = "";
                foreach (Process pTest in p) {
                    if(pTest.MainWindowTitle != "") {
                        nowPlaying = pTest.MainWindowTitle;
                    }
                }
                
                Regex rgx = new Regex("Spotify...|Spotify");
                String mainOut = rgx.Replace(nowPlaying, "");

                String pat = @"^\s";
                Regex psd = new Regex(pat);
                Match m = psd.Match(mainOut);
                if(m.Success) { mainOut = "*PAUSED*"; }

                String responseString = mainOut;

                curr = "Got connection, Sending: \n" + mainOut;

                byte[] buffer2 = System.Text.Encoding.UTF8.GetBytes(responseString);
                byte[] buffer = new byte[buffer2.Length + 3];
                buffer2.CopyTo(buffer, 3);
                buffer[0] = 0xEF;
                buffer[1] = 0xBB; 
                buffer[2] = 0xBF;

                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (ProtocolViolationException ex)
            {
                curr = "Someone sent us bad connection, SADAFEC. :(((";
            }
            listen();
        }

        static void Main(string[] args)
        {
            Console.SetWindowSize(50, 6);
            Console.SetBufferSize(50, 6);
            T.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            T.Interval = 100;
            T.Enabled = true;
            start();
            Console.Read();
        }
    }
}
