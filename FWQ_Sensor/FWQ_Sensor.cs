using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using Confluent.Kafka;
using System.Threading;

namespace FWQ_Sensor
{
    class FWQ_Sensor
    {
        static void Main(string[] args)
        {
            String nombreAtraccion = "";
            String bootstrapServ = "";
            String nombreTopic = "";
            if (args.Length == 0)
            {
                Console.WriteLine("Introduce el nombre de la aplicacion: ");
                nombreAtraccion = Console.ReadLine();
                Console.WriteLine("Introduce el bootstrapServer: ");
                bootstrapServ = Console.ReadLine();
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--bootstrap-server")
                    {
                        bootstrapServ = args[i + 1];
                    }
                    if (args[i] == "--name")
                    {
                        nombreAtraccion = args[i + 1];
                    }
                }
            }
            var config = new ProducerConfig
            {

                BootstrapServers = bootstrapServ,

            };
            Console.WriteLine("Introduce el topic: ");
            nombreTopic = Console.ReadLine();
            Action<DeliveryReport<Null, string>> handler = r =>
            Console.WriteLine(!r.Error.IsError
            ? $"Delivered message to {r.TopicPartitionOffset}"
            : $"Delivery Error: {r.Error.Reason}");

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                while (true)
                {
                    Random r = new Random();
                    Random p = new Random();
                    int texto = r.Next(1,100);
                    producer.Produce(nombreTopic, new Message<Null, string> { Value = nombreAtraccion + " " + texto.ToString() }) ;
                    producer.Flush(TimeSpan.FromSeconds(2));
                    int ms = p.Next(1, 4);
                    Thread.Sleep(ms * 1000);
                }
            }
        }
    }
}
