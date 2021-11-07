using Confluent.Kafka;
using System;
//using Microsoft.Extensions.Configuration;
using System.Net;

class Producer
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
            for(int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--bootstrap-server")
                {
                    bootstrapServ = args[i + 1];
                }
                if(args[i] == "--topic")
                {
                    nombreTopic = args[i + 1];
                }
            }
            if (bootstrapServ == "")
            {
                Console.Write("Por favor, introduce el bootstrap Server: ");
                bootstrapServ = Console.ReadLine();
            }
            if(nombreTopic == "")
            {
                Console.Write("Introduzca el nombre del topic: ");
                nombreTopic = Console.ReadLine();
            }
        }
        var config = new ProducerConfig
        {
            
            BootstrapServers = bootstrapServ,
            
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
                Console.Write("Introduzca un texto (escribe \"SALIR\" para finalizar): ");
                texto = Console.ReadLine();
                if(texto != "SALIR")
                {
                    producer.Produce(nombreTopic, new Message<Null, string> { Value = texto });
                    numProduced++;
                }
            } while (texto != "SALIR");  
            producer.Flush(TimeSpan.FromSeconds(10));
            Console.WriteLine($"{numProduced} mensajes han sido producidos para el topic sd-events");
        }
    }
}