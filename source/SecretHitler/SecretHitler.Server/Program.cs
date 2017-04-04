using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;

namespace SecretHitler.Server
{
    /// <summary>
    /// Server app main console class
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point for server console app
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // TODO Make configurable.
            ServerUrl = "http://127.0.0.1:8888";
            SignalR = WebApp.Start(ServerUrl);
            var proxy = ClientProxy.Instance;

            string cmd;
            do
            {
                cmd = Console.ReadLine();
            }
            while (cmd != "quit");
        }

        /// <summary>
        /// Owin WebApp instance
        /// </summary>
        private static IDisposable SignalR { get; set; }

        /// <summary>
        /// The URL where the server will be hosted.
        /// </summary>
        public static string ServerUrl { get; private set; }
    }
}
