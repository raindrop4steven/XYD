using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(XYD.Startup))]

namespace XYD
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Storage is the only thing required for basic configuration.
            // Just discover what configuration options do you have.
        }
    }
}
