using Microsoft.Owin.Cors;
using Owin;

namespace SecretHitler.Server
{
    /// <summary>
    /// Used by OWIN's startup process. 
    /// </summary>
    class Startup
    {
        /// <summary>
        /// Configuration for Owin.
        /// </summary>
        /// <param name="app">AppBuilder object</param>
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }
}
