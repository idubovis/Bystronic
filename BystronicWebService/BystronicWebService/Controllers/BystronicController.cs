using BystronicWebService.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;

namespace BystronicWebService.Controllers
{
    abstract public class BystronicController : ControllerBase
    {
        protected IBystronicRepository _repository;

        public BystronicController()
        {
            SetRepository(new ProductionBystronicRepository());
        }

        public void SetRepository(IBystronicRepository repository)
        {
            _repository = repository;
        }

        protected JObject GetRequestBody()
        {
            string body = null;
            using (var stream = new StreamReader(Request.Body))
            {
                Request.Body.Seek(0, SeekOrigin.Begin);
                body = stream.ReadToEnd();
            }
            return body != null ? JObject.Parse(body) : null;
        }
    }
}
