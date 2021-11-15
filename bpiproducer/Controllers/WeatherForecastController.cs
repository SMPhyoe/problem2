using bpiproducer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace producer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BPIController : ControllerBase
    {

        [HttpPost]
        public async Task PostAsync()
        {
            var factory = new ConnectionFactory()
            {   //HostName = "localhost" , 
                //Port = 30724
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RABBITMQ_PORT"))
            };

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(" https://api.coindesk.com/v1/bpi/currentprice.json"))
                {
                    BPI bpi = new BPI();
                    bpi.Message = await response.Content.ReadAsStringAsync();

                    Console.WriteLine(factory.HostName + ":" + factory.Port);
                    using (var connection = factory.CreateConnection())
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueDeclare(queue: "Bpi",
                                             durable: false,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);

                        //string message = bpi.Message;
                        var body = Encoding.UTF8.GetBytes(bpi.Message);

                        channel.BasicPublish(exchange: "",
                                             routingKey: "Bpi",
                                             basicProperties: null,
                                             body: body);
                    }

                }
            }

            
        }
    }
}
