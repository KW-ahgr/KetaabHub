using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Translator.MServ.Api.Shared;

namespace Translator.MServ.Api
{
    public sealed class RabbitMqTranslationRpcClient : IDisposable
    {
        private readonly ILogger<RabbitMqTranslationRpcClient> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _requestQueueName = "translation_rpc_queue";
        private readonly string _replyQueueName;
        private readonly EventingBasicConsumer _consumer;

        private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> _pending =
            new ConcurrentDictionary<string, TaskCompletionSource<string>>();

        public RabbitMqTranslationRpcClient(ILogger<RabbitMqTranslationRpcClient> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASS") ?? "guest",
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // اطمینان از وجود صف RPC
            _channel.QueueDeclare(queue: _requestQueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // صف پاسخ اختصاصی برای همین کلاینت (server-named)
            _replyQueueName = _channel.QueueDeclare(queue: "", durable: false, exclusive: true, autoDelete: true, arguments: null).QueueName;

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += async (_, ea) =>
            {
                try
                {
                    var corrId = ea.BasicProperties?.CorrelationId ?? string.Empty;
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    if (_pending.TryRemove(corrId, out var tcs))
                    {
                        // می‌تونی اینجا Deserialize هم بکنی؛ برای سادگی مستقیم متن رو برمی‌گردونیم
                        var response = JsonSerializer.Deserialize<TranslateTextResponse>(json);
                        tcs.TrySetResult(response?.TranslatedText ?? json);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing RPC reply.");
                }

                await Task.Yield();
            };

            _channel.BasicConsume(queue: _replyQueueName, autoAck: true, consumer: _consumer);
        }

        public async Task<string> RequestTranslationAsync(string text, string toLanguage, TimeSpan? timeout = null, CancellationToken ct = default)
        {
            var corrId = Guid.NewGuid().ToString("N");
            var props = _channel.CreateBasicProperties();
            props.CorrelationId = corrId;
            props.ReplyTo = _replyQueueName;

            var message = new TranslateTextRequest(text, toLanguage);
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!_pending.TryAdd(corrId, tcs))
                throw new InvalidOperationException("Duplicate correlation id.");

            _channel.BasicPublish(exchange: "", routingKey: _requestQueueName, basicProperties: props, body: body);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(10));

            await using (cts.Token.Register(() =>
            {
                if (_pending.TryRemove(corrId, out var pending))
                    pending.TrySetException(new TimeoutException("Translation RPC timeout."));
            })) { }

            return await tcs.Task.ConfigureAwait(false);
        }

        public void Dispose()
        {
            try { _channel?.Close(); } catch { }
            try { _connection?.Close(); } catch { }
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
