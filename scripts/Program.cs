using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;

class Program
{
    static async Task Main(string[] args)
    {
        var bootstrapServer = "localhost:9092";
        var topic = "portfolio.transactions";
        var count = 1;
        var transactionType = 1; // 1=Buy, 2=Sell
        var assetType = 1; // 1=Share, 2=Bond, 3=Crypto

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
                case "--count" when i + 1 < args.Length:
                    count = int.Parse(args[++i]);
                    break;
                case "--transaction-type" when i + 1 < args.Length:
                    transactionType = int.Parse(args[++i]);
                    break;
                case "--asset-type" when i + 1 < args.Length:
                    assetType = int.Parse(args[++i]);
                    break;
            }
        }

        Console.WriteLine("==========================================");
        Console.WriteLine("Отправка тестового сообщения в Kafka");
        Console.WriteLine("==========================================");
        Console.WriteLine($"Bootstrap Server: {bootstrapServer}");
        Console.WriteLine($"Topic: {topic}");
        Console.WriteLine($"Количество сообщений: {count}");
        Console.WriteLine();

        // Проверка доступности Kafka перед отправкой
        Console.WriteLine("Проверка доступности Kafka...");
        try
        {
            using var testClient = new System.Net.Sockets.TcpClient();
            var parts = bootstrapServer.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 ? int.Parse(parts[1]) : 9092;

            var connectTask = testClient.ConnectAsync(host, port);
            if (await Task.WhenAny(connectTask, Task.Delay(2000)) == connectTask && testClient.Connected)
            {
                testClient.Close();
                Console.WriteLine($"✅ Kafka доступен на {bootstrapServer}");
            }
            else
            {
                Console.WriteLine($"❌ ОШИБКА: Kafka недоступен на {bootstrapServer}");
                Console.WriteLine();
                Console.WriteLine("Возможные причины:");
                Console.WriteLine("  1. Kafka не запущен");
                Console.WriteLine("  2. Неправильный адрес или порт");
                Console.WriteLine("  3. Проблемы с сетью или firewall");
                Console.WriteLine();
                Console.WriteLine("Проверьте:");
                Console.WriteLine($"  - Запущен ли Kafka: Get-Process | Where-Object {{$_.ProcessName -like '*kafka*'}}");
                Console.WriteLine($"  - Доступен ли порт: Test-NetConnection {host} -Port {port}");
                Console.WriteLine();
                Console.WriteLine("📖 Инструкции по запуску Kafka см. в файле: scripts/KAFKA_SETUP.md");
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ОШИБКА при проверке доступности Kafka: {ex.Message}");
            Console.WriteLine($"Проверьте, что Kafka запущен на {bootstrapServer}");
            Console.WriteLine();
            Console.WriteLine("📖 Инструкции по запуску Kafka см. в файле: scripts/KAFKA_SETUP.md");
            Environment.Exit(1);
        }

        Console.WriteLine();

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServer,
            // Увеличиваем таймауты для более понятных ошибок
            SocketTimeoutMs = 5000,
            RequestTimeoutMs = 5000
        };

        using var producer = new ProducerBuilder<string, string>(config).Build();

        int successCount = 0;
        int failCount = 0;

        for (int i = 0; i < count; i++)
        {
            var transactionId = Guid.NewGuid().ToString();
            var portfolioId = Guid.NewGuid().ToString();
            var stockCardId = Guid.NewGuid().ToString();
            var transactionTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var message = new
            {
                id = transactionId,
                portfolioId = portfolioId,
                stockCardId = stockCardId,
                assetType = assetType,
                transactionType = transactionType,
                quantity = 100,
                pricePerUnit = 250.75m,
                totalAmount = 25075.00m,
                transactionTime = transactionTime,
                currency = "RUB",
                metadata = (string?)null
            };

            var messageJson = JsonSerializer.Serialize(message);

            Console.WriteLine($"[{i + 1}/{count}] Отправка сообщения...");
            Console.WriteLine($"   Transaction ID: {transactionId}");
            Console.WriteLine($"   Portfolio ID: {portfolioId}");
            Console.WriteLine($"   Stock Card ID: {stockCardId}");

            try
            {
                var result = await producer.ProduceAsync(topic, new Message<string, string>
                {
                    Key = portfolioId,
                    Value = messageJson
                });

                Console.WriteLine($"   ✅ Сообщение успешно отправлено!");
                Console.WriteLine($"      Topic: {result.Topic}");
                Console.WriteLine($"      Partition: {result.Partition}");
                Console.WriteLine($"      Offset: {result.Offset}");
                successCount++;
            }
            catch (ProduceException<string, string> ex)
            {
                Console.WriteLine($"   ❌ Ошибка Kafka: {ex.Error.Reason}");
                failCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Ошибка: {ex.Message}");
                failCount++;
            }

            Console.WriteLine();
        }

        producer.Flush(TimeSpan.FromSeconds(10));

        Console.WriteLine("==========================================");
        Console.WriteLine($"Результат: {successCount} успешно, {failCount} ошибок");
        Console.WriteLine("==========================================");
        Console.WriteLine();
        Console.WriteLine("💡 Проверьте логи AnalyticsService для подтверждения обработки");
        Console.WriteLine("💡 Проверьте базу данных: SELECT * FROM asset_transactions ORDER BY transaction_time DESC LIMIT 10;");
    }
}
