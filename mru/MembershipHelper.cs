using System;
using System.Web.Security;
using System.Configuration;

namespace Bespoke.Utils.Security
{
    [Serializable]
    public class MembershipHelper
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string UserName { get; set; }
        public string ConnectionString { get; set; }

        public void AddUser()
        {
            try
            {
                if (Membership.FindUsersByName(this.UserName).Count == 0)
                {
                    MembershipUser user = Membership.CreateUser(this.UserName, this.Password, this.Email);
                    Console.WriteLine("User {0} has been added", user.UserName);
                }
                else
                {
                    Console.WriteLine("The user {0} already exists", this.UserName);
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
            }

        }


        internal void SomeMethod()
        {
            Console.WriteLine("Conn : {0}", ConfigurationManager.ConnectionStrings[0].ConnectionString);
            Console.WriteLine("Test : {0}", ConfigurationManager.AppSettings["test"]);
            Console.WriteLine("User Name : {0}", this.UserName);
        }
    }
}
