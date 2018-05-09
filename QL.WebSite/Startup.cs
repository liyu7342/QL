using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QL.WebSite.Startup))]
namespace QL.WebSite
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
