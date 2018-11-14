 using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Web.Http;
using WebApi.Models;




namespace WebApi.Controllers
{
    public class equityController : ApiController
    {

         

            public IEnumerable<equity> GetAllProducts()
            {


            private const int PORT = 12345;


        string data;
        IPEndPoint serverAddress;
        NetworkStream networkstream;
        StreamReader streamreader;
        StreamWriter streamwriter;
        string input;

        serverAdress = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);

                Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                                                      ProtocolType.Tcp);
                   server.Connect(serverAddress);
                
        

networkstream = new NetworkStream(server);
streamreader = new StreamReader(networkstream);
streamwriter = new StreamWriter(networkstream);

var data = streamreader.ReadLine();         // Read welcome from Server
                                            // Console.WriteLine(data);

        List<equity> equities = new  List<equity>();

                while (true)                // While communicating with Server
                {
                    input = Console.ReadLine();         // Read text from Console
                    if (input == "exit")                // Close connection to Server?
                        break;
                    //streamwriter.WriteLine(input);      // Send text to Server
                    streamwriter.Flush();

                    data = streamreader.ReadLine();     // Read echo from Server
                    equintities. Add(data)           // Write echo to Console
                }

                Console.WriteLine("Closing connection to Server...");
                streamreader.Close();
                streamwriter.Close();
                networkstream.Close();
                server.Shutdown(SocketShutdown.Both);
                server.Close();

            
    

        //equity[] equities = new equity[]
        //    {
        //    new equity { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 },
        //    new equity  { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M },
        //    new equity  { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M }
        //    };
                return equities;
            }
               
            public IHttpActionResult saveProductinDB(int id)
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
                    SqlCommand command = new SqlCommand("insert into equity " + "values('" + equities[i].ToString() + "', '')" , sql);
                    command.ExecuteNonQuery();
                }
                sql.Close();

            }
            return Ok(product);
            }
 
