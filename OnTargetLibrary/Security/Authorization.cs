using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace OnTargetLibrary.Security
{
    public class IntranetSecurityGroupRequirement : IAuthorizationRequirement
    {
        public string IntranentSecurityGroup { get; set; }
        public IntranetSecurityGroupRequirement(string intranetSecurityGroup)
        {
            IntranentSecurityGroup = intranetSecurityGroup;
        }
    }
}
