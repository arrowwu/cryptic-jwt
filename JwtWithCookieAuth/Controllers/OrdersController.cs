using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Bson;
using JwtWithCookieAuth.Models;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JwtWithCookieAuth.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    [Route("api/orders")]
    public class OrdersController : Controller
    {
        DataAccess _dataAccess;

        public OrdersController()
        {
            _dataAccess = new DataAccess();
        }
        // GET api/orders
        [HttpGet]
        public IActionResult Get()
        {
            var orders = _dataAccess.GetOrders();
            var json = new { orders = orders };
            return new OkObjectResult(json);
        }

        // GET api/orders?category=toy
        [HttpGet("query")]
        public IActionResult Get([FromQuery]String category, [FromHeader]String jwt)
        {
            var emailFromJwt = GetValueFromJwtPayload(jwt);
            if (emailFromJwt == null || emailFromJwt != "admin")
            {
                return Unauthorized();
            };
            IEnumerable<Order> orders;
            if (category == null)
            {
                return BadRequest();
            }
            else
            {
                orders = _dataAccess.GetOrders(category);
            }          
            var json = new { orders = orders, ordersCount = orders.ToArray().Length };
            return new OkObjectResult(json);
        }

        [HttpGet("{id:length(24)}")]
        [ResponseCache(Duration = 60)]
        public IActionResult GetById(string id, [FromHeader]String authorization)
        {
            var emailFromJwt = GetValueFromJwtPayload(authorization);
            if (emailFromJwt == null)
            {
                return Unauthorized();
            };
            Order order = _dataAccess.GetOrder(id);           
            if (order == null)
            {
                return NotFound();
            }
            if(order.Owner.Email != emailFromJwt)
            {
                return Unauthorized();
            }
            var json = new { order = order };
            //           Request.HttpContext.Response.Headers.Add("X-Total-Count", "4");
            return new OkObjectResult(json);
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]OrderFormOrder orderFormOrder, [FromHeader]String authorization)
        {
            var emailFromJwt = GetValueFromJwtPayload(authorization);
            if (emailFromJwt == null || orderFormOrder.Owner != emailFromJwt)
            {
                return Unauthorized();
            };
            Order order = new Order();
            order.OrderName = orderFormOrder.OrderName;
            order.Price = orderFormOrder.Price;
            order.Category = orderFormOrder.Category;
            order.Timestamp = DateTime.UtcNow.ToString();
            await _dataAccess.Create(order, emailFromJwt);

            // Construct the return json body and then return it with 201 Created response
            orderFormOrder.Id = order.Id;
            orderFormOrder.Timestamp = order.Timestamp;
            var json = new { order = orderFormOrder };
            string location = order.Id.ToString();
            return Created(location, json);
        }

        // PUT api/orders/59c44d7f06333b978cb7ef69
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Put(String id, [FromBody]OrderFormOrder orderFormOrder, [FromHeader]String authorization)
        {
            var emailFromJwt = GetValueFromJwtPayload(authorization);
            if (emailFromJwt == null || orderFormOrder.Owner != emailFromJwt)
            {
                return Unauthorized();
            };
            Order order = new Order();
            order.Id = id;
            order.OrderName = orderFormOrder.OrderName;
            order.Price = orderFormOrder.Price;
            order.Category = orderFormOrder.Category;
            order.Timestamp = DateTime.UtcNow.ToString();
            await _dataAccess.Update(id, order);

            // Construct the return json body and then return it with 200 Ok response
            orderFormOrder.Id = order.Id;
            orderFormOrder.Timestamp = order.Timestamp;
            var json = new { order = orderFormOrder };
            return new OkObjectResult(json);
        }

        // DELETE api/orders/59c44d7f06333b978cb7ef69
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id, [FromHeader]String authorization)
        {
            var emailFromJwt = GetValueFromJwtPayload(authorization);
            await _dataAccess.Remove(id, emailFromJwt);
            return new OkResult();
        }

        private String GetValueFromJwtPayload(String jwt)
        {
            //Assume the input is in a control called txtJwtIn,
            //and the output will be placed in a control called txtJwtOut
            var jwtHandler = new JwtSecurityTokenHandler();

            //Check if readable token (string is in a JWT format)
            var readableToken = jwtHandler.CanReadToken(jwt.Substring(7));

            if (readableToken != true)
            {
                return null;
            }

            var token = jwtHandler.ReadJwtToken(jwt.Substring(7));

            //Extract the payload of the JWT
            var claims = token.Claims;
            foreach (Claim c in claims)
            {
                if(c.Type == ClaimTypes.Email)
                {
                    return c.Value;
                }
            }
            return null;
            
        }


    }
}