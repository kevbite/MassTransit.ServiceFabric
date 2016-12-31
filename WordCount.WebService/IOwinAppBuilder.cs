using Owin;

namespace Kevsoft.WordCount.WebService
{
    public interface IOwinAppBuilder
    {
        void Configuration(IAppBuilder appBuilder);
    }
}