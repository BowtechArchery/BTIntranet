using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnTargetLibrary.Security
{
    public class MultiplePolicysAuthorizeAttribute : TypeFilterAttribute
    {
        public MultiplePolicysAuthorizeAttribute(string policys, bool isAnd = false) : base(typeof(MultiplePolicysAuthorizeFilter))
        {
            Arguments = new object[] { policys, isAnd };
        }
    }
}
