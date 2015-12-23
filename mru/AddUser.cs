using System;

namespace Bespoke.Utils.Security
{
    public class AspAddUser 
    {
        public string TaskAssemblyDirectory { get; set; }

        // Methods
        public bool Execute()
        {
            var domain = Program.CreateAppDomain(ConfigurationFile, TaskAssemblyDirectory);

            var mh = new MembershipHelper
            {
                UserName = UserName,
                Password = Password,
                Email = Email
            };

            domain.DoCallBack(mh.AddUser);


            foreach (string r in Roles)
            {
                var rh = new RolesHelpher
                {
                    UserName = UserName,
                    Role = r
                };
                domain.DoCallBack(rh.AddRole);
                domain.DoCallBack(rh.AssignUserToRole);

            }

            AppDomain.Unload(domain);

            return true;
        }
        
        public string ConfigurationFile { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string[] Roles { get; set; }
        public string UserName { get; set; }
    }
 

}
