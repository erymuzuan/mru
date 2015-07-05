using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

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
            if (ParseArguements(args, roles, out userName, out password, out email, out configurationFile))
            {
                AppDomain domainSecurityHelper = CreateAppDomain(configurationFile, null);

                var mh = new MembershipHelper { UserName = userName, Password = password, Email = email };

                domainSecurityHelper.DoCallBack(mh.AddUser);


                foreach (string r in roles)
                {
                    var rh = new RolesHelpher { UserName = userName, Role = r };
                    domainSecurityHelper.DoCallBack(rh.AddRole);
                    domainSecurityHelper.DoCallBack(rh.AssignUserToRole);

                }


                AppDomain.Unload(domainSecurityHelper);
                return;

            }
            //
            // add roles to existing users
            if (ParseArguements(args, roles, out userName, out configurationFile))
            {
                AppDomain domainSecurityHelper = CreateAppDomain(configurationFile, null);
                foreach (string r in roles)
                {
                    var rh = new RolesHelpher { UserName = userName, Role = r };
                    domainSecurityHelper.DoCallBack(rh.AddRole);
                    domainSecurityHelper.DoCallBack(rh.AssignUserToRole);

                }

                AppDomain.Unload(domainSecurityHelper);
                return;

            }

            //  function to add just roles
            roles.Clear();
            if (ParseArguements(args, roles, out configurationFile))
            {
                AppDomain domainSecurityHelper = CreateAppDomain(configurationFile, null);
                foreach (string r in roles)
                {
                    var rh = new RolesHelpher { Role = r };
                    domainSecurityHelper.DoCallBack(rh.AddRole);

                }
                AppDomain.Unload(domainSecurityHelper);
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
            string appBase = assemblyPath ?? new Uri(
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
            AppDomain domainSecurityHelper = AppDomain.CreateDomain("Security Helper", null, ads);
            return domainSecurityHelper;
        }

        private static void Usage()
        {
            const ConsoleColor YELLOW = ConsoleColor.Yellow;
            ConsoleColor defaultColor = Console.ForegroundColor;

            Console.ForegroundColor = YELLOW;
            Console.WriteLine("Usage :\t\tTo add user \r\n\t\t mru -u <username> -p <password> -e <email> [-r] <rolename> -c <configurationFile>\r\n");
            Console.WriteLine("Usage :\t\tTo add user to a role \r\n\t\t mru -r <rolename> -u <username> -c <configurationFile>\r\n");
            Console.WriteLine("Usage :\t\tTo add role \r\n\t\t mru -r <rolename> -c <configurationFile>\r\n");

            Console.ForegroundColor = defaultColor;


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

        static bool ParseArguements(string[] args, List<string> roles, out string userName, out string password, out string email, out string configurationFile)
        {
            userName = string.Empty;
            password = string.Empty;
            email = string.Empty;
            if (null == roles) roles = new List<string>();
            configurationFile = string.Empty;

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-u":
                        userName = args[i + 1];
                        break;
                    case "-p":
                        password = args[i + 1];
                        break;
                    case "-e":
                        email = args[i + 1];
                        break;
                    case "-r":
                        roles.Add(args[i + 1]);
                        break;
                    case "-c":
                        string config = args[i + 1];
                        configurationFile = Path.IsPathRooted(config) ? config :
                                Path.Combine(Environment.CurrentDirectory, config);

                        break;
                    default:
                        break;
                }
            }

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
                    default:
                        break;
                }
            }

            return !string.IsNullOrEmpty(userName)
                && !string.IsNullOrEmpty(configurationFile)
                && File.Exists(configurationFile);
        }
    }
}
