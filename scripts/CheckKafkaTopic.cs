using Confluent.Kafka;
using System;
using System.Linq;
using System.Text;

namespace CheckKafkaTopic
{
    class Program
    {
        static void Main(string[] args)
        {
            var bootstrapServer = "localhost:9092";
            var topic = "portfolio.transactions";
            var maxMessages = 10;
            var fromBeginning = false;

            // Парсинг аргументов
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--bootstrap-server" when i + 1 < args.Length:
                        bootstrapServer = args[++i];
                        break;
                    case "--topic" when i + 1 < args.Length:
                        topic = args[++i];
                        break;
                    case "--max-messages" when i + 1 < args.Length:
                        if (int.TryParse(args[++i], out var maxMsg))
                        {
                            maxMessages = maxMsg;
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Неверное значение для --max-messages: {args[i]}, используется значение по умолчанию: 10");
                        }
                        break;
                    case "--from-beginning":
                        fromBeginning = true;
                        break;
                    default:
                        // Если аргумент не начинается с --, считаем его позиционным
                        if (!args[i].StartsWith("--"))
                        {
                            if (i == 0) bootstrapServer = args[i];
                            else if (i == 1) topic = args[i];
                            else if (i == 2 && int.TryParse(args[i], out var posMaxMsg))
                            {
                                maxMessages = posMaxMsg;
                            }
                        }
                        break;
                }
            }

            Console.WriteLine("==========================================");
            Console.WriteLine("Проверка сообщений в топике Kafka");
            Console.WriteLine("==========================================");
            Console.WriteLine($"Bootstrap Server: {bootstrapServer}");
            Console.WriteLine($"Topic: {topic}");
            Console.WriteLine($"Max Messages: {maxMessages}");
            Console.WriteLine();

            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServer,
                GroupId = $"check-topic-{Guid.NewGuid()}",
                AutoOffsetReset = fromBeginning ? AutoOffsetReset.Earliest : AutoOffsetReset.Latest, // Читаем с начала или только новые
                EnableAutoCommit = false,
                // Настройки для чтения новых сообщений
                FetchWaitMaxMs = 1000 // Максимальное время ожидания для Fetch
            };

            try
            {
                using var consumer = new ConsumerBuilder<string, string>(config).Build();
                consumer.Subscribe(topic);

                Console.WriteLine($"Подписка на топик '{topic}' выполнена");
                Console.WriteLine("Ожидание сообщений...");
                Console.WriteLine();

                var count = 0;
                var timeout = TimeSpan.FromSeconds(10); // Увеличиваем таймаут для ожидания новых сообщений
                var startTime = DateTime.UtcNow;
                var maxWaitTime = TimeSpan.FromSeconds(30); // Максимальное время ожидания новых сообщений

                Console.WriteLine("Ожидание новых сообщений (до 30 секунд)...");
                Console.WriteLine("(Сообщения, отправленные до подписки, не будут прочитаны)");
                Console.WriteLine();

                while (count < maxMessages && (DateTime.UtcNow - startTime) < maxWaitTime)
                {
                    try
                    {
                        var result = consumer.Consume(timeout);

                        if (result == null)
                        {
                            if (count == 0 && (DateTime.UtcNow - startTime) < TimeSpan.FromSeconds(5))
                            {
                                // Продолжаем ждать, если еще не прошло 5 секунд
                                continue;
                            }
                            else if (count == 0)
                            {
                                Console.WriteLine("⚠️ Новых сообщений в топике не найдено");
                                Console.WriteLine("💡 Попробуйте отправить сообщение после запуска проверки");
                                Console.WriteLine("💡 Или используйте --from-beginning для чтения всех сообщений");
                            }
                            break;
                        }

                        count++;
                        Console.WriteLine($"Сообщение #{count}:");
                        Console.WriteLine($"  Topic: {result.Topic}");
                        Console.WriteLine($"  Partition: {result.Partition}");
                        Console.WriteLine($"  Offset: {result.Offset}");
                        Console.WriteLine($"  Key: {result.Message?.Key ?? "null"}");
                        Console.WriteLine($"  Value: {result.Message?.Value ?? "null"}");
                        Console.WriteLine($"  IsPartitionEOF: {result.IsPartitionEOF}");
                        Console.WriteLine($"  Timestamp: {result.Message?.Timestamp.UtcDateTime:O}");

                        if (result.Message?.Headers != null && result.Message.Headers.Count > 0)
                        {
                            Console.WriteLine($"  Headers: {string.Join(", ", result.Message.Headers.Select(h => $"{h.Key}={System.Text.Encoding.UTF8.GetString(h.GetValueBytes())}"))}");
                        }
                        Console.WriteLine();
                    }
                    catch (ConsumeException ex)
                    {
                        Console.WriteLine($"Ошибка при получении сообщения: {ex.Error.Reason} (Code: {ex.Error.Code})");
                        break;
                    }
                }

                if (count > 0)
                {
                    Console.WriteLine($"Всего получено сообщений: {count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
