using System;
using System.IO;
using System.Linq;

using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace AWRP {
    public class Connector {
        private ServiceClient client;
        
        private bool _IsReady;
        
        public bool IsReady {
            get {
                return _IsReady;
            }
            private set {
                _IsReady = value;
            }
        }

        public Connector(string domain, string username, string secret) {
            string connectionString = $@"
AuthType = OAuth;
Url = {domain};
UserName = {username};
Password = {secret};
LoginPrompt=Auto;
RequireNewInstance = True";
            try {
                client = new ServiceClient(connectionString);
                IsReady = client.IsReady;
            } catch {
                IsReady = false;
            }
        }

        private string EncodeFile(string file) {
            FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[fs.Length];
            long count = fs.Read(buffer, 0, (int)fs.Length);
            fs.Close();
            return Convert.ToBase64String(buffer); 
        }

        public bool UploadFile(string file, string resourcePath, string description, string solutionUniqueName) {
            if (!File.Exists(file)) {
                Console.WriteLine($"File '{file}' dont exists");
                return false;
            }

            if (!SolutionExists(solutionUniqueName)) {
                    Console.WriteLine($"A solution with the unique id '{solutionUniqueName}' does not exists");
                    return false;
            }

            string resourceName =  resourcePath + Path.GetFileName(file);

            Entity entity = new Entity("webresource");
            entity["content"] = EncodeFile(file);
            entity["displayname"] = Path.GetFileName(file);
            entity["description"] = description;
            entity["name"] = resourceName;

            switch(Path.GetExtension(file)) {
                    case ".html":
                    case ".htm":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Html);
                        break;
                    case ".css":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Style);
                        break;
                    case ".js":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Script);
                        break;
                    case ".png":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Png);
                        break;
                    case ".jpg":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Jpg);
                        break;
                    case ".gif":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Gif);
                        break;
                    case ".xap":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Silverlight);
                        break;
                    case ".xsl":
                    case ".xslt":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.StyleSheet);
                        break;
                    case ".ico":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Ico);
                        break;
                    case ".svg":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.Vector);
                        break;
                    case ".resx":
                        entity["webresourcetype"] = new OptionSetValue((int)WebResourceType.String);
                        break;
                    default:
                        return false;
            }

            QueryByAttribute qba = new QueryByAttribute("webresource");
            qba.ColumnSet = new ColumnSet(true);
            qba.AddAttributeValue("name", resourceName);
            Entity res;

            if ((res = client.RetrieveMultiple(qba).Entities.FirstOrDefault()) == null) {
                CreateRequest req = new CreateRequest() {
                    Target = entity
                };
                req.Parameters.Add("SolutionUniqueName", solutionUniqueName);

                Console.WriteLine("WebResource dont exists... Automatically create one");
                client.Execute(req);
            } else {
                Console.WriteLine("WebResource exists... Try to update");
                entity.Id = res.Id;
                client.Update(entity);
            }
            return true;
        }

        private bool SolutionExists(string solutionUniqueName) { 
            QueryByAttribute qba = new QueryByAttribute("solution");
            qba.AddAttributeValue("uniquename", solutionUniqueName);
            EntityCollection res = client.RetrieveMultiple(qba);
            return res.Entities.Count != 0;
        }
    }
}