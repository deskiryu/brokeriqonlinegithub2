using System;
using System.Collections.Generic;
using System.Text;

namespace BrokerIQ.Online.Services.Interface
{
    using BrokerIQ.Online.Models;

    public interface IAlertService
    {
        event Action<AlertBIQ> OnAlert;
        void Success(string message, bool keepAfterRouteChange = false, bool autoClose = true);
        void Error(string message, bool keepAfterRouteChange = false, bool autoClose = true);
        void Info(string message, bool keepAfterRouteChange = false, bool autoClose = true);
        void Warn(string message, bool keepAfterRouteChange = false, bool autoClose = true);
        void Alert(AlertBIQ alert);
        void Clear(string id = null);
    }
}
