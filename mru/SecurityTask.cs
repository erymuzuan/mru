

namespace Bespoke.Utils.Security
{

    public class AspAddRole
    {
        public string TaskAssemblyDirectory { get; set; }


        public bool Execute()
        {
           var secDomain =  Program.CreateAppDomain(ConfigurationFile, TaskAssemblyDirectory);
           foreach (var r in Roles)
           {
               var rh = new RolesHelpher {Role = r};
               secDomain.DoCallBack(rh.AddRole);
           }

           return true;
        }
        
        public string ConfigurationFile { get; set; }
        public string[] Roles { get; set; }
    }

}
