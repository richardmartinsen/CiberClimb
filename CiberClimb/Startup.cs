using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CiberClimb.Startup))]
namespace CiberClimb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
