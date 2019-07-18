using System.Collections.Generic;
using BystronicWebService.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace BystronicWebService.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class OrdersController : BystronicController
    {
        // GET api/orders
        [HttpGet]
        public ActionResult<IEnumerable<Order>> Get()
        {
            return Ok(_repository.GetAllOrders(Startup.BystronicData));
        }

        // GET api/orders/id
        [HttpGet("{id}")]
        public ActionResult<Order> Get(int id)
        {
            var order = _repository.GetAllOrders(Startup.BystronicData).Find(o => o.ID == id);
            if (order != null)
                return Ok(order);
            return BadRequest(id);
        }

        // POST api/orders/5
        [HttpPost("{id}")]
        public void Post(int id, [FromBody] string value)
        {
            var exists = _repository.GetAllOrders(Startup.BystronicData).Exists(o => o.ID == id);
            var data = GetRequestBody().ToObject<Dictionary<string, object>>();
            var order = new Order(data, Startup.BystronicData);
            if (exists)
                _repository.UpdateOrder(order);
            else
                _repository.AddOrder(order);
        }

        // DELETE api/orders/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _repository.DeleteOrder(id);
        }
    }
}
