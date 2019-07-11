using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Net.NetworkInformation;

namespace OnTargetLibrary.Security
{
    public class IntranetSecurityGroupHandler : AuthorizationHandler<IntranetSecurityGroupRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IntranetSecurityGroupRequirement requirement)
        {

            var username = context.User.Identity.Name;

            string domain = IPGlobalProperties.GetIPGlobalProperties().DomainName;

            using (var ctx = new PrincipalContext(ContextType.Domain, domain))
            {
                var user =  UserPrincipal.FindByIdentity(ctx, username);

                if (user == null)
                {
                    return Task.CompletedTask;
                }
                else
                {

                    PrincipalSearchResult<Principal> groups = user.GetAuthorizationGroups();

                    // iterate over all groups for user
                    foreach (GroupPrincipal group in groups)
                    {

                        if (requirement.IntranentSecurityGroup == group.Name)
                        {
                            context.Succeed(requirement);
                        }

                    }

                }
            }

            return Task.CompletedTask;
        }
    }
}
