using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using BystronicWebService.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BystronicWebService.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class LoginController : BystronicController
    {
        // POST api/login
        [HttpPost]
        public ActionResult<IEnumerable<Product>> Post([FromBody] string value)
        {
            var identity = GetBasicIdentity(Request);
            var request = GetRequestBody();
            if (IsAuthorized(identity))
            {
                var user = GetUser(identity);
                return Ok(user);
            }
            else
                return Unauthorized();
        }

        private HttpListenerBasicIdentity GetBasicIdentity(HttpRequest req)
        {
            try
            {
                if(!req.Headers.ContainsKey("Authorization")) return null;
                var auth = req.Headers["Authorization"].First<string>().Trim();
                if (!auth.StartsWith("Basic") || auth.Length < 6 || !Char.IsWhiteSpace(auth[5])) return null;
                var base64 = auth.Substring(6).TrimStart();
                var userAndPass = Encoding.ASCII.GetString(Convert.FromBase64String(base64));
                int colon = userAndPass.IndexOf(':');
                if (colon < 0) return null;
                return new HttpListenerBasicIdentity(userAndPass.Substring(0, colon), userAndPass.Substring(colon + 1));
            }
            catch
            {
                return null;
            }
        }

        private bool IsAuthorized(HttpListenerBasicIdentity identity)
        {
            //var ppp = new ClaimsPrincipal(identity);
            bool aaa = LoginController.IsInGroup(identity, @"LCHPID1\Editors");
            bool bbb = LoginController.IsInGroup(identity, @"BUILTIN\Administrators");

            return true;
        }

        public static bool IsInGroup(IIdentity identity, string groupName)
        {
            try
            {
                var myIdentity = new WindowsIdentity(identity.Name);
                var groupNames = (from id in myIdentity.Groups
                                  select id.Translate(typeof(NTAccount)).Value);

                return groupNames.Contains(groupName);
            }
            catch(Exception)
            {
                return false;
            }
        }


        private ResponseMessage GetUser(HttpListenerBasicIdentity identity)
        {
            Permissions permissions = null;
            BystronicUser user = _repository.GetAllSalesmen().Find(x => x.ID.Equals(identity.Name, StringComparison.InvariantCultureIgnoreCase));
            if (user != null)
            {
                permissions = new Permissions();
            }
            else
            {
                user = new BystronicUser(identity.Name.ToLower(), identity.Name, BystronicUser.ROLE_ADMINISTRATOR, "Midwest");
                permissions = new Permissions();
            }

            var views = _repository.GetViews(user.ID);
            var columns = new List<string>();

            return new ResponseMessage { Status = true, Query = "login", Data = new Dictionary<string, object> { { "Service Info", "2.0.Alpha"/*GetVersionInfo()*/ }, { "User", user }, { "Permissions", permissions }, { "Columns", columns }, { "Views", views } } };
        }
    }
}
