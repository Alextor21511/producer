using Confluent.Kafka;
using System;
//using Microsoft.Extensions.Configuration;
using System.Net;
using System.Threading;

namespace consumer
{
    class consumer
    {
        static void Main(string[] args)
        {
            string bootstrapServ = "";
            string nombreTopic = "";
            //En caso de que no se introduzcan valores como argumento, cogemos la configuracion por defecto:
            if (args.Length == 0)
            {
                Console.Write("Por favor, introduce el bootstrap Server: ");
                bootstrapServ = Console.ReadLine();
                Console.Write("Introduzca el nombre del topic: ");
                nombreTopic = Console.ReadLine();
            }
            else
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--bootstrap-server")
                    {
                        bootstrapServ = args[i + 1];
                    }
                    if (args[i] == "--topic")
                    {
                        nombreTopic = args[i + 1];
                    }
                }
                if (bootstrapServ == "")
                {
                    Console.Write("Por favor, introduce el bootstrap Server: ");
                    bootstrapServ = Console.ReadLine();
                }
                if (nombreTopic == "")
                {
                    Console.Write("Introduzca el nombre del topic: ");
                    nombreTopic = Console.ReadLine();
                }
            }


            var config = new ConsumerConfig
            {

                BootstrapServers = bootstrapServ,
                GroupId = "console-consumer-43044"

            };

            using (var consumer = new ConsumerBuilder<Null, string>(config) 
            .Build())
            {
                consumer.Subscribe("sd-events");

                while (true)
                {
                    try
                    {
                        ConsumeResult<Null, string> consumeResult = consumer.Consume();

                        if (consumeResult != null)
                        {
                            if (consumeResult.IsPartitionEOF)
                            {
                                Console.WriteLine($"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");

                                continue;
                            }

                            Console.WriteLine($"Received message at {consumeResult.TopicPartitionOffset}: {consumeResult.Value}");

                            Console.WriteLine($"Received message => {consumeResult.Value}");

                        }
                    }

                    catch (ConsumeException e)
                    {
                        Console.WriteLine($"Consume error: {e.Error.Reason}");
                    }
                }

            }
            
        }
            
    }
}

