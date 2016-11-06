using System.IO;
using GitHot.Core;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace GitHot.Bootstrap
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override IRootPathProvider RootPathProvider
        {
            get
            {
                return new AspNetRootPathProvider();
            }
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            pipelines.BeforeRequest.AddItemToEndOfPipeline(ctx =>
            {
                Configuration.InstancePath = Path.Combine(RootPathProvider.GetRootPath(), "App_Data");
                return null;
            });
        }

        protected override void RequestStartup(TinyIoCContainer container, IPipelines pipelines, NancyContext context)
        {

            // Enable CORS
            pipelines.AfterRequest.AddItemToEndOfPipeline((ctx) =>
            {
                ctx.Response.WithHeader("Access-Control-Allow-Origin", "*")
                    .WithHeader("Access-Control-Allow-Methods", "GET")
                    .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-Type");
            });
        }
    }
}