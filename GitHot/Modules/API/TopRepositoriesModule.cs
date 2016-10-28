using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using GitHot.Core;

namespace GitHot.Modules
{
    public class TopRepositoriesModule : NancyModule
    {
        public TopRepositoriesModule(IRootPathProvider pathProvider)
        {
            Get["/repos/{criteria}/{weeks}"] = param =>
            {
                string criteria = (string)(param["criteria"]);
                char[] letters = criteria.ToCharArray();

                if (!Char.IsUpper(letters[0]))
                {
                    letters[0] = Char.ToUpper(letters[0]);
                    criteria = new string(letters);
                }

                string filepath = Path.Combine(pathProvider.GetRootPath(), $"App_Data/repos/{criteria}/{param["weeks"]}.json");

                string json;
                Response resp;

                RepositoryCriteria crit;
                if (File.Exists(filepath))
                {
                    using (StreamReader sr = new StreamReader(filepath))
                    {
                        json = sr.ReadToEnd();
                        resp = json;
                        resp.StatusCode = HttpStatusCode.OK;
                    }
                }
                else if (!Enum.TryParse(criteria, out crit))
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