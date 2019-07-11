using System.Text;
using System.DirectoryServices.AccountManagement;
using System.Security.Claims;
using System.Security.Principal;
using System.Collections.Generic;
using System;

namespace OnTargetLibrary.Security
{
    public static class User
    {
        public static string GetUserFullName(string username)
        {
            using (var context = new PrincipalContext(ContextType.Domain, "bowtech"))
            {
                var user = UserPrincipal.FindByIdentity(context, username);
                if (user != null)
                {
                    //ViewData["UserName"] = user.Name;
                    //ViewData["EmailAddress"] = user.EmailAddress;

                    return user.Name;
                }
                else
                {
                    return "";
                }
            }
        }

        public static bool IsInGroup(this ClaimsPrincipal User, string GroupName)
        {
            var groups = new List<string>();

            var wi = (WindowsIdentity)User.Identity;
            if (wi.Groups != null)
            {
                foreach (var group in wi.Groups)
                {
                    try
                    {
                        groups.Add(group.Translate(typeof(NTAccount)).ToString());
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                return groups.Contains(GroupName);
            }
            return false;
        }

    }



}