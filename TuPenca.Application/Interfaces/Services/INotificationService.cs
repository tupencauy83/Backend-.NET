using System;
using System.Collections.Generic;
using System.Text;

namespace TuPenca.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task EnviarAsync(string fcmToken, string titulo, string cuerpo);
    }
}