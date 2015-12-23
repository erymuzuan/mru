using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Linq;

namespace Bespoke.Utils.Security
{
    public class Program
    {

        static void Main(string[] args)
        {
            string userName, password, email, configurationFile;
            var roles = new List<string>();

            //
            // create a user with optional role
            if (ParseArguements(roles, out userName, out password, out email, out configurationFile))
            {
                var domain = CreateAppDomain(configurationFile, null);

                var mh = new MembershipHelper { UserName = userName, Password = password, Email = email };
                domain.DoCallBack(mh.AddUser);


                foreach (var r in roles)
                {
                    var rh = new RolesHelpher { UserName = userName, Role = r };
                    domain.DoCallBack(rh.AddRole);
                    domain.DoCallBack(rh.AssignUserToRole);

                }


                AppDomain.Unload(domain);
                return;

            }
            //
            // add roles to existing users
            if (ParseArguements(args, roles, out userName, out configurationFile))
            {
                var domain = CreateAppDomain(configurationFile, null);
                foreach (var r in roles)
                {
                    var rh = new RolesHelpher { UserName = userName, Role = r };
                    domain.DoCallBack(rh.AddRole);
                    domain.DoCallBack(rh.AssignUserToRole);

                }

                AppDomain.Unload(domain);
                return;

            }

            //  function to add just roles
            roles.Clear();
            if (ParseArguements(args, roles, out configurationFile))
            {
                var domain = CreateAppDomain(configurationFile, null);

                foreach (var r in roles)
                {
                    var rh = new RolesHelpher { Role = r };
                    domain.DoCallBack(rh.AddRole);

                }
                AppDomain.Unload(domain);
                return;
            }

            Usage();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationFile"></param>
        /// <param name="assemblyPath">null for command line, for MSBuild task , where's the task assembly is</param>
        /// <returns></returns>
        public static AppDomain CreateAppDomain(string configurationFile, string assemblyPath)
        {
            var appBase = assemblyPath ?? new Uri(
                // ReSharper disable AssignNullToNotNullAttribute
                Path.GetDirectoryName(
                Path.GetFullPath(Assembly.GetEntryAssembly().Location))).AbsoluteUri;
            // ReSharper restore AssignNullToNotNullAttribute


            // set up
            var ads = new AppDomainSetup
            {
                ApplicationBase = appBase,
                DisallowBindingRedirects = false,
                DisallowCodeDownload = true,
                ConfigurationFile = configurationFile
            };

            // Create the second AppDomain.
            var domain = AppDomain.CreateDomain("Security Helper", null, ads);
            if (!ParseArgExist("env")) return domain;

            domain.DoCallBack(AddConnectionString2);
            return domain;
        }

        private static void AddConnectionString2()
        {
            var connName = ParseArg("conn") ?? "Sph";
            var appName = ParseArg("app");
            var connectionString = GetEnvironmentVariable("SqlConnectionString", appName);
            AddConnectionString(connName, connectionString);

        }

        private static void Usage()
        {
            const ConsoleColor YELLOW = ConsoleColor.Yellow;
            var defaultColor = Console.ForegroundColor;
            const string TAB = "\t\t";
            const string RETURN = "\r\n";
            Console.ForegroundColor = YELLOW;
            Console.WriteLine($"Usage :{TAB}To add user {RETURN}{TAB} mru -u <username> -p <password> -e <email> [-r] <rolename> -c <configurationFile>{RETURN}");
            Console.WriteLine($"Usage :{TAB}To add user to a role {RETURN}{TAB} mru -r <rolename> -u <username> -c <configurationFile>{RETURN}");
            Console.WriteLine($"Usage :{TAB}To add role {RETURN}{TAB} mru -r <rolename> -c <configurationFile>{RETURN}");
            Console.WriteLine($"Usage :{TAB}To add SqlConnectionString from Environment variable use -env and optionally the variable name and connection name");
            Console.WriteLine($"Usage :{TAB}-app <the application name>() and -conn <the name for connection string> the default is sph");

            Console.ForegroundColor = defaultColor;


        }

        public static string ParseArg(string name)
        {
            var args = Environment.CommandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var index = Array.IndexOf(args, $"-{name}");
            if (index == -1) return null;

            return index <= (args.Length - 2) ? args[index + 1] : null;
        }
        public static string[] ParseArgs(string name)
        {
            var args = Environment.CommandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var roles = new List<string>();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-" + name)
                    roles.Add(args[i + 1]);
            }
            return roles.ToArray();

        }

        private static bool ParseArgExist(string name)
        {
            var args = Environment.CommandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return args.Any(a => a.StartsWith($"-{name}"));
        }

        static bool ParseArguements(string[] args, List<string> roles, out string configurationFile)
        {
            configurationFile = string.Empty;
            if (null == roles) roles = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-r":
                        roles.Add(args[i + 1]);
                        break;
                    case "-c":
                        string config = args[i + 1];
                        configurationFile = Path.IsPathRooted(config) ? config : Path.Combine(Environment.CurrentDirectory, config);
                        break;
                }
            }

            return roles.Count != 0
                && !string.IsNullOrEmpty(configurationFile)
                && File.Exists(configurationFile);
        }

        static bool ParseArguements(List<string> roles, out string userName, out string password, out string email, out string configurationFile)
        {
            userName = ParseArg("u");
            password = ParseArg("p");
            email = ParseArg("e");
            roles?.AddRange(ParseArgs("r"));
            var config = ParseArg("c");
            configurationFile = Path.IsPathRooted(config) ? config :
                    Path.Combine(Environment.CurrentDirectory, config);

            return !string.IsNullOrEmpty(password)
                && !string.IsNullOrEmpty(userName)
                && !string.IsNullOrEmpty(email)
                && !string.IsNullOrEmpty(configurationFile)
                && File.Exists(configurationFile);
        }

        static bool ParseArguements(string[] args, List<string> roles, out string userName, out string configurationFile)
        {
            userName = string.Empty;
            if (null == roles) roles = new List<string>();
            configurationFile = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-u":
                        userName = args[i + 1];
                        break;
                    case "-r":
                        roles.Add(args[i + 1]);
                        break;
                    case "-c":
                        string config = args[i + 1];
                        configurationFile = Path.IsPathRooted(config) ? config :
                                Path.Combine(Environment.CurrentDirectory, config);

                        break;
                }
            }

            return !string.IsNullOrEmpty(userName)
                && !string.IsNullOrEmpty(configurationFile)
                && File.Exists(configurationFile);
        }


        private static string GetEnvironmentVariable(string setting, string app)
        {
            var process = Environment.GetEnvironmentVariable($"RX_{app}_{setting}");
            if (!string.IsNullOrWhiteSpace(process)) return process;

            var user = Environment.GetEnvironmentVariable($"RX_{app}_{setting}", EnvironmentVariableTarget.User);
            if (!string.IsNullOrWhiteSpace(user)) return user;

            return Environment.GetEnvironmentVariable($"RX_{app}_{setting}", EnvironmentVariableTarget.Machine);
        }

        public static void AddConnectionString(string connectionName, string connectionString)
        {
            var settings = ConfigurationManager.ConnectionStrings;
            var element = typeof(ConfigurationElement).GetField("_bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            var collection = typeof(ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

            element.SetValue(settings, false);
            collection.SetValue(settings, false);

            settings.Add(new ConnectionStringSettings(connectionName, connectionString));

            // Repeat above line as necessary

            collection.SetValue(settings, true);
            element.SetValue(settings, true);

        }
    }
}
