using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace GitHot.Modules.API
{
    public class TopOrganizationsModule : NancyModule
    {
        public TopOrganizationsModule(IRootPathProvider pathProvider)
        {
            Get["/orgs/{criteria}/{weeks}"] = param =>
            {
                string criteria = ((string)param.criteria).ToLower();

                string filepath = Path.Combine(pathProvider.GetRootPath(), $"App_Data/orgs/{criteria}/{param["weeks"]}.json");

                string json;
                Response resp;

                if (File.Exists(filepath))
                {
                    using (StreamReader sr = new StreamReader(filepath))
                    {
                        json = sr.ReadToEnd();
                        resp = json;
                        resp.StatusCode = HttpStatusCode.OK;
                    }
                }
                else if (criteria != "total" && criteria != "avg")
                {
                    json = "{ error: 'Invalid criteria'}";
                    resp = json;
                    resp.StatusCode = HttpStatusCode.OK;
                }
                else
                {
                    json = "{ message: 'Data file not found. Processing request'}";
                    resp = json;
                    resp.StatusCode = HttpStatusCode.Accepted;
                }

                resp.ContentType = "application/json";
                return resp;
            };
        }
    }
}