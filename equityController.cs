using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Models;




namespace WebApi.Controllers
{
    public class equityController : ApiController
    {
 
 
       
            equity[] equities = new equity[]
            {
            new equity { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 },
            new equity  { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M },
            new equity  { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M }
            };

            public IEnumerable<equity> GetAllProducts()
            {
                return equities;
            }

            public IHttpActionResult GetProduct(int id)
            {
                var product = equities.FirstOrDefault((p) => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            else
            {

                SqlConnection sql = new SqlConnection("server=localhost; uid=xxxx; pwd=xxxx; database=Movies;");
                sql.Open();

                for (int i = 0; i < equities.Length; i++)
                {
                    SqlCommand command = new SqlCommand("insert into equity " + "values('" + equities[i].ToString() + "', '')"+"order by name", sql);
                    command.ExecuteNonQuery();
                }
                sql.Close();

            }
            return Ok(product);
            }
        }
    }
}
}
