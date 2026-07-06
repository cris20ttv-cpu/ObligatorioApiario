using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ObligatorioApiario.Data;
using ObligatorioApiario.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace ObligatorioApiario.Services
{
    public class TelegramBotService : BackgroundService
    {
        private readonly ILogger<TelegramBotService> _logger;
        private readonly TelegramBotClient _botClient;
        private readonly IServiceProvider _serviceProvider;
        
        // State machine for `/crearvisita`
        private static readonly ConcurrentDictionary<long, VisitCreationState> _userStates = new();

        public TelegramBotService(ILogger<TelegramBotService> logger, IConfiguration config, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            
            var token = config["TelegramBotToken"];
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No TelegramBotToken found in configuration!");
                token = "DUMMY"; // Prevents crash, but bot won't work
            }
            
            _botClient = new TelegramBotClient(token);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_botClient.BotId == null && _botClient.Timeout.TotalSeconds == 100)
            {
                 _logger.LogInformation("Bot token missing or invalid.");
            }

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation("Telegram Bot started!");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Message is not { } message)
                {
                    if (update.CallbackQuery is { } callbackQuery)
                    {
                        await HandleCallbackQueryAsync(botClient, callbackQuery, cancellationToken);
                    }
                    return;
                }

                if (message.Text is not { } messageText)
                    return;

                var chatId = message.Chat.Id;

                if (messageText.StartsWith("/start") || messageText.StartsWith("/ayuda"))
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "🐝 *¡Bienvenido a Zánganos Bot!*\n\nComandos disponibles:\n/pendientes - Ver visitas sin completar\n/crearvisita - Programar una nueva visita de campo",
                        parseMode: ParseMode.Markdown,
                        cancellationToken: cancellationToken);
                    return;
                }

                if (messageText.StartsWith("/pendientes"))
                {
                    await HandlePendientesCommand(botClient, chatId, cancellationToken);
                    return;
                }

                if (messageText.StartsWith("/crearvisita"))
                {
                    await HandleCrearVisitaStart(botClient, chatId, cancellationToken);
                    return;
                }

                await botClient.SendTextMessageAsync(chatId, "Comando no reconocido. Prueba con /ayuda", cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling update");
            }
        }

        private async Task HandlePendientesCommand(ITelegramBotClient botClient, long chatId, CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var visitas = await db.Visitas
                .Include(v => v.Tareas)
                .Include(v => v.ApiariosRevisados)
                    .ThenInclude(r => r.Apiario)
                .ToListAsync(ct);

            var pendientes = visitas.Where(v => !v.Tareas.Any() || v.Tareas.Any(t => t.Estado != "Completada")).ToList();

            if (!pendientes.Any())
            {
                await botClient.SendTextMessageAsync(chatId, "🎉 ¡No hay visitas pendientes! Todo al día.", cancellationToken: ct);
                return;
            }

            var response = $"📋 *Tienes {pendientes.Count} visitas pendientes:*\n\n";
            foreach (var p in pendientes)
            {
                var apiario = p.ApiariosRevisados.FirstOrDefault()?.Apiario?.Nombre ?? "Sin Apiario";
                response += $"🔸 *VST-{p.ID_Visita:D5}* | {apiario}\n🗓 {p.Fecha:dd/MM/yyyy} | Tareas: {p.Tareas.Count}\n\n";
            }

            await botClient.SendTextMessageAsync(chatId, response, parseMode: ParseMode.Markdown, cancellationToken: ct);
        }

        private async Task HandleCrearVisitaStart(ITelegramBotClient botClient, long chatId, CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var apiarios = await db.Apiarios.OrderBy(a => a.Nombre).ToListAsync(ct);
            if (!apiarios.Any())
            {
                await botClient.SendTextMessageAsync(chatId, "No hay apiarios registrados.", cancellationToken: ct);
                return;
            }

            _userStates[chatId] = new VisitCreationState();

            var buttons = new List<InlineKeyboardButton[]>();
            for(int i=0; i < apiarios.Count; i+=2)
            {
                if (i + 1 < apiarios.Count)
                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(apiarios[i].Nombre, $"CV_API_{apiarios[i].ID_Apiario}"), InlineKeyboardButton.WithCallbackData(apiarios[i+1].Nombre, $"CV_API_{apiarios[i+1].ID_Apiario}") });
                else
                    buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(apiarios[i].Nombre, $"CV_API_{apiarios[i].ID_Apiario}") });
            }
            
            var keyboard = new InlineKeyboardMarkup(buttons);

            await botClient.SendTextMessageAsync(chatId, "¿Para qué *Apiario* quieres programar la visita?", parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: ct);
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken ct)
        {
            var chatId = callbackQuery.Message!.Chat.Id;
            var data = callbackQuery.Data;

            if (data == null) return;

            if (!_userStates.TryGetValue(chatId, out var state))
            {
                await botClient.SendTextMessageAsync(chatId, "Sesión caducada. Escribe /crearvisita de nuevo.", cancellationToken: ct);
                return;
            }

            if (data.StartsWith("CV_API_"))
            {
                state.ApiarioId = int.Parse(data.Replace("CV_API_", ""));
                
                var seasons = new[] { "Primavera", "Verano", "Otoño", "Invierno" };
                var buttons = seasons.Select(s => new[] { InlineKeyboardButton.WithCallbackData(s, $"CV_TEM_{s}") }).ToList();
                var keyboard = new InlineKeyboardMarkup(buttons);

                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Apiario seleccionado. ¿Qué *Temporada* es?", parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: ct);
            }
            else if (data.StartsWith("CV_TEM_"))
            {
                state.Temporada = data.Replace("CV_TEM_", "");

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var apicultores = await db.Apicultores.ToListAsync(ct);

                var buttons = apicultores.Select(a => new[] { InlineKeyboardButton.WithCallbackData(a.Nombre, $"CV_APC_{a.CIApicultor}") }).ToList();
                var keyboard = new InlineKeyboardMarkup(buttons);

                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"Temporada seleccionada. ¿Qué *Apicultor* realizará la visita?", parseMode: ParseMode.Markdown, replyMarkup: keyboard, cancellationToken: ct);
            }
            else if (data.StartsWith("CV_APC_"))
            {
                state.ApicultorId = int.Parse(data.Replace("CV_APC_", ""));

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var visita = new Visita
                {
                    Fecha = DateTime.UtcNow,
                    Temporada = state.Temporada,
                    CIApicultor = state.ApicultorId,
                    Observaciones = "Visita generada automáticamente desde Telegram."
                };

                visita.ApiariosRevisados.Add(new Revisa
                {
                    ID_Apiario = state.ApiarioId,
                    H_Llegada = TimeSpan.FromHours(8),
                    H_Salida = TimeSpan.FromHours(10),
                    Clima = "Despejado"
                });

                var colmenas = await db.Colmenas.Include(c => c.Instalaciones).Where(c => c.Instalaciones.Any(i => i.ID_Apiario == state.ApiarioId)).ToListAsync(ct);
                if (colmenas.Any())
                {
                    foreach (var c in colmenas)
                    {
                        visita.Tareas.Add(new TareaVisita
                        {
                            ID_Colmena = c.ID_Colmena,
                            Descripcion = "Revisión general",
                            Estado = "Pendiente"
                        });
                    }
                }

                db.Visitas.Add(visita);
                await db.SaveChangesAsync(ct);

                _userStates.TryRemove(chatId, out _);

                await botClient.EditMessageTextAsync(chatId, callbackQuery.Message.MessageId, $"✅ ¡Visita creada con éxito!\nID de visita: VST-{visita.ID_Visita:D5}", cancellationToken: ct);
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            _logger.LogError(ErrorMessage);
            return Task.CompletedTask;
        }

        class VisitCreationState
        {
            public int ApiarioId { get; set; }
            public string Temporada { get; set; } = string.Empty;
            public int ApicultorId { get; set; }
        }
    }
}
