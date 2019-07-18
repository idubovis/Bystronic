using System.Collections.Generic;
using BystronicWebService.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace BystronicWebService.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class DataController : BystronicController
    { 
        // GET api/data
        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get()
        {
            var user = new BystronicUser(JObject.Parse(Request.Query["user"]).ToObject<Dictionary<string, object>>());
            return Ok(BystronicData.GetData(_repository).serializeResponseMessage(user));
        }

        // GET api/data/info
        [HttpGet("{info}")]
        public ActionResult<Product> Get(string info)
        {
            var infoData = new { Service = "Bystronic Web Service", Version = "2.0"};
            return BadRequest(infoData);
        }
    }
}
