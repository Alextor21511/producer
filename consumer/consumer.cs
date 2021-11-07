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



            var config = new ConsumerConfig
            {

                BootstrapServers = "localhost:9092",
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

