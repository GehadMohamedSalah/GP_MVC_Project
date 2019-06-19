using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QueueSystemWeb.Startup))]
namespace QueueSystemWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
