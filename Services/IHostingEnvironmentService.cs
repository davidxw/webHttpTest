using webHttpTest.Models;

namespace webHttpTest.Services
{
    public interface IHostingEnvironmentService
    {
        HostingEnvironment GetHostingEnvironment();

        string PrintHostingEnvironment();
    }
}
