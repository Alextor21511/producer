using Confluent.Kafka;
using System;
//using Microsoft.Extensions.Configuration;
using System.Net;

class Producer
{
    static void Main()
    {
        var config = new ProducerConfig
        {
            
            BootstrapServers = "localhost:9092",
            
        };
        
        Action<DeliveryReport<Null, string>> handler = r =>
        Console.WriteLine(!r.Error.IsError
        ? $"Delivered message to {r.TopicPartitionOffset}"
        : $"Delivery Error: {r.Error.Reason}");

        using (var producer = new ProducerBuilder<Null, string>(config).Build())
        {
            String texto;
            int numProduced=0;
            do
            {
                Console.WriteLine("Introduzca un texto (escribe \"SALIR\" para finalizar)");
                texto = Console.ReadLine();
                if(texto != "SALIR")
                {
                    producer.Produce("sd-events", new Message<Null, string> { Value = texto });
                    numProduced++;
                }
            } while (texto != "SALIR");  
            producer.Flush(TimeSpan.FromSeconds(10));
            Console.WriteLine($"{numProduced} mensajes han sido producidos para el topic sd-events");
        }
    }
}