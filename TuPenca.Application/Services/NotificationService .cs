using FirebaseAdmin.Messaging;
using TuPenca.Application.Interfaces.Services;

namespace TuPenca.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        public async Task EnviarAsync(string fcmToken, string titulo, string cuerpo)
        {
            var message = new Message
            {
                Token = fcmToken,
                Notification = new Notification
                {
                    Title = titulo,
                    Body = cuerpo
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}