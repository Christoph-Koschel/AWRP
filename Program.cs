using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace AWRP {
    public class Program {
        private static string CONFIG_PATH;

        private static void Usage(Assembly asm) {
            Console.Write("Usage: ");
            Console.Write(Path.ChangeExtension(asm.GetName().Name,".exe"));
            Console.WriteLine(" [init|add|push|remove|help] [options]");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("DESCRIPTION");
            Console.WriteLine($"    {asm.GetName().Name} is a tool to upload PowerApps WebResources dynamically to a solution.");
            Console.WriteLine($"    Initialize a new connection for your password using your host (organization>.crm.dynamics.com), a user, and his user secret.");
            Console.WriteLine($"    Adding files using the 'add' command and push them dynamically to your existing solution.");
            Console.WriteLine();
            Console.WriteLine("COMMANDS");
            Console.WriteLine("    init");
            Console.WriteLine("        Initialize a new connection in the current working directory");
            Console.WriteLine();
            Console.WriteLine("    add");
            Console.WriteLine("        Add a file to the tracking list");
            Console.WriteLine();
            Console.WriteLine("        OPTIONS");
            Console.WriteLine("            -f <file>");
            Console.WriteLine("            The file relative to the config file that should be uploaded");
            Console.WriteLine("            -s <solution>");
            Console.WriteLine("            The Solutions unique name");
            Console.WriteLine("            [-p <prefix>]");
            Console.WriteLine("            When the resource path prefix is not set, the default of the config file is used");
            Console.WriteLine("            [-d <description>]");
            Console.WriteLine("            The description of the WebResource");
            Console.WriteLine();
            Console.WriteLine("    remove");
            Console.WriteLine("        Removes a file from the tracking list");
            Console.WriteLine();
            Console.WriteLine("        OPTIONS");
            Console.WriteLine("            -f <file>");
            Console.WriteLine("            The file relative to the config file that should be uploaded");
            Console.WriteLine();
            Console.WriteLine("    push");
            Console.WriteLine("        Upload all tracked files to your environment");
            Console.WriteLine();
            Console.WriteLine("    help");
            Console.WriteLine("        Prints this text");
            Console.WriteLine();
        }

        public static int Main(string[] args) {
            if (args.Length == 0) {
                Assembly asm = Assembly.GetEntryAssembly();
                Console.Write(asm.GetName().Name);
                Console.Write(" ");
                Console.WriteLine(asm.GetName().Version);
                Console.WriteLine();
                Usage(asm);
                return 0;
            }

            CONFIG_PATH = Path.Combine(Env.CWD, Config.CONFIG_NAME);

            string task = args[0];

            if (task == "init") {
                return Init();
            }
            if (task == "push") {
                return Push();
            }
            if (task == "add") {
                string file = null;
                string prefix = null;
                string solution = null;
                string description = "";

                for (uint i = 1; i < args.Length; i++)
                {
                    if (args[i] == "-f") {
                        if (i + 1 >= args.Length) {break;}
                        file = args[++i];
                    } else if (args[i] == "-d") {
                        if (i + 1 >= args.Length) {break;}
                        description = args[++i];
                    } else if (args[i] == "-p") {
                        if (i + 1 >= args.Length) {break;}
                        prefix = args[++i];
                    } else if (args[i] == "-s") {
                        if (i + 1 >= args.Length) {break;}
                        solution = args[++i];
                    }
                }

                if (file == null) {
                    Console.WriteLine("Missing parameter '-f <file>'");
                    return 1;
                }
                if (solution == null) {
                    Console.WriteLine("Missing parameter '-s <solution>'");
                    return 1;
                }

                return Add(file, description, prefix, solution);
            }
            if (task == "remove") {
                string file = null;

                for (uint i = 0; i < args.Length; i++) {
                    if (args[i] == "-f") {
                        if (i + 1 >= args.Length) {break;}
                        file = args[++i];
                    }
                }

                if (file == null) {
                    Console.WriteLine("Missing parameter '-f <file>'");
                    return 1;
                }

                return Remove(file);
            }

            {  
                Assembly asm = Assembly.GetEntryAssembly();
                Console.Write(asm.GetName().Name);
                Console.Write(" ");
                Console.WriteLine(asm.GetName().Version);
                Console.WriteLine();
                Usage(asm);
                return 0;
            }
        }

        private static int Init() {
            Config config = new Config();
            config.Uploads = new List<Config.UploadItem>();

            Console.Write("Host: ");
            config.Host = Console.ReadLine();
            config.Host = config.Host.StartsWith("http") ? config.Host : "https://" + config.Host;
            Console.Write("User: ");
            config.User = Console.ReadLine();
            Console.Write("Password: ");
            config.Password = Console.In.ReadPassword();
            Console.Write("Resource prefix: ");
            config.Prefix = Console.ReadLine();

            if (config.Prefix.Length == 0) {
                Console.WriteLine($"Unallowed prefix: '{config.Prefix}'");
            }

            if (!config.Prefix.EndsWith("_")) {
                config.Prefix += "_";
            }

            if (config.Password == Config.PASSWORD_NULL) {
                Console.WriteLine("Password cannot be null");
                return 1;
            }
           

            Console.WriteLine($"Try to connect to host '{config.Host}'...");
            Connector connector = new Connector(config.Host, config.User, config.Password);
            if (connector.IsReady) {
                Console.WriteLine("Successfully connected to Server");

                config.Password = Console.In.Decision("Save the password? (Not secure)") ? config.Password : Config.PASSWORD_NULL;
                Config.Write(CONFIG_PATH, config);   
            } else {
                Console.WriteLine($"Connection to the host '{config.Host}' failed");
                return 1;
            }
            return 0;
        }

        private static int Push() {            
            if (File.Exists(CONFIG_PATH)) {
                if (!Config.Load(CONFIG_PATH, out Config config)) {
                    Console.WriteLine("Could not load config file. Please re-init the project");
                    return 1;
                }

                if (config.Password == Config.PASSWORD_NULL) {
                    Console.Write("Password: ");
                    config.Password = Console.In.ReadPassword();
                }

                Connector connector = new Connector(config.Host, config.User, config.Password);
                if (!connector.IsReady) {
                    Console.WriteLine($"Connection to the host '{config.Host}' failed");
                    return 1;
                }
    
                foreach(Config.UploadItem item in config.Uploads) {
                    Console.WriteLine($"Try uploading '{item.Src}'");
                    connector.UploadFile(Path.Combine(Env.CWD, item.Src), item.ResoucrePath, item.Description, item.SolutionUniqueName); 
                }
            } else {
                Console.WriteLine("Nothing is initilized use the 'init' commnad");
                return 1;
            }
            
            return 0;
        }

        private static int Add(string file, string description, string prefix, string solutionUniqueName) {
            string filePath = Path.Combine(Env.CWD, file);

            if (!File.Exists(filePath)) {
                Console.WriteLine($"Cannot find file '{file}'");
                return 1;
            }

            if (File.Exists(CONFIG_PATH)) {
                if (!Config.Load(CONFIG_PATH, out Config config)) {
                    Console.WriteLine("ERROR: Could not load config file. Please re-init the project");
                    return 1;
                }

                file = file.Replace('\\', '/');
                file = file.StartsWith("./") ? file.Substring(2) : file;

                Config.UploadItem item = new Config.UploadItem() {
                    Src = file,
                    ResoucrePath = prefix ?? config.Prefix,
                    Description = description,
                    SolutionUniqueName = solutionUniqueName
                };
                
                config.Uploads.Add(item);
                Config.Write(CONFIG_PATH, config);
                Console.WriteLine($"Added '{file}' to upload list");
            } else {
                Console.WriteLine("Nothing is initilized use the 'init' command");
                return 1;
            }
            
            return 0;
        }

        private static int Remove(string file) {
            if (File.Exists(CONFIG_PATH)) {
                if (!Config.Load(CONFIG_PATH, out Config config)) {
                    Console.WriteLine("ERROR: Could not load config file. Please re-init the project");
                    return 1;
                }

                int toRemove = -1;

                file = file.Replace('\\', '/');
                file = file.StartsWith("./") ? file.Substring(2) : file;

                for (int i = 0; i < config.Uploads.Count; i++)
                {
                    if (config.Uploads[i].Src == file) {
                        toRemove = i;
                        break;
                    }
                }

                if (toRemove != -1) {
                    Console.WriteLine($"Successfully removed '{file}'");
                    config.Uploads.RemoveAt(toRemove);

                    Config.Write(CONFIG_PATH, config);
                } else {
                    Console.WriteLine($"Could not find file '{file}' in your config");
                    return 1;
                }
            } else {
                Console.WriteLine("Nothing is initilized use the 'init' command");
                return 1;
            }

            return 0;
        }
    }
}