using Nancy;

namespace GitHot.Modules
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/"] = param => View["Index.sshtml"];
        }
    }
}