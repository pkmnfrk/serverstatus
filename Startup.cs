using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ServerStatus.Startup))]
namespace ServerStatus
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
