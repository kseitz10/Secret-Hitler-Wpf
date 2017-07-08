using System;
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
            // TODO Make configurable, handle running as admin problem.
            ServerUrl = "http://*:8888";
            SignalR = WebApp.Start(ServerUrl);
            var proxy = Director.Instance;

            string cmd;
            do
            {
                cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "start":
                        ServerHub.StateMachine.Start();
                        break;
                    default:
                        Console.WriteLine("Unsupported command. Try \"start\".");
                        break;
                }
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
