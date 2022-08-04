using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(TeckyGenesis.Areas.Identity.IdentityHostingStartup))]
namespace TeckyGenesis.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
            });
        }
    }
}