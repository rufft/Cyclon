namespace Cyclone.Common.SimpleSoftDelete.RabbitMQ;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string User { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    /// <summary>Имя обменника для событий soft-delete.</summary>
    public string Exchange { get; set; } = "SoftDeleteExchange";

    /// <summary>Необязательное имя очереди сервиса (по умолчанию — имя приложения).</summary>
    public string? QueueName { get; set; }
}