using System.Collections.Generic;
using BystronicWebService.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BystronicWebService.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class ProductsController : BystronicController
    {
        // GET api/products
        [HttpGet]
        public ActionResult<IEnumerable<Product>> Get()
        {
            return Ok(_repository.GetProducts());
        }

        // GET api/products/id
        [HttpGet("{id}")]
        public ActionResult<Product> Get(int id)
        {
            var product = _repository.GetProducts().Find(p => p.ID == id);
            if (product != null)
                return Ok(product);
            return BadRequest(id);
        }

        // POST api/products/5
        [HttpPost("{id}")]
        public void Post(int id, [FromBody] string value)
        {
            var exists = _repository.GetProducts().Exists(p => p.ID == id);
            var productJSON = GetRequestBody();
            var data = productJSON.ToObject<Dictionary<string, object>>();
            var product = new Product(data);
            if (exists)
                _repository.UpdateProduct(product);
            else
                _repository.AddProduct(product);
        }

        // DELETE api/products/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            var product = _repository.GetProducts().Find(p => p.ID == id);
            if (product != null)
                _repository.DeleteProduct(product);
        }
    }
}
