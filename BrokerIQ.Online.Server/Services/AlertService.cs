using System;
namespace BrokerIQ.Online.Services
{
    using BrokerIQ.Online.Models;
    using BrokerIQ.Online.Services.Interface;


    public class AlertService : IAlertService
    {
        private const string _defaultId = "default-alert";
        public event Action<AlertBIQ> OnAlert;

        public void Success(string message, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(new AlertBIQ
            {
                Type = AlertType.Success,
                Message = message,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
                Delay = 5000
            });
        }        

        public void Error(string message, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(new AlertBIQ
            {
                Type = AlertType.Error,
                Message = message,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
                Delay = 10000
            });
        }        

        public void Info(string message, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(new AlertBIQ
            {
                Type = AlertType.Info,
                Message = message,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
                Delay = 5000
            });
        }        

        public void Warn(string message, bool keepAfterRouteChange = false, bool autoClose = true)
        {
            this.Alert(new AlertBIQ
            {
                Type = AlertType.Warning,
                Message = message,
                KeepAfterRouteChange = keepAfterRouteChange,
                AutoClose = autoClose,
                Delay = 5000
            });
        }        

        public void Alert(AlertBIQ alert)
        {
            alert.Id = alert.Id ?? _defaultId;
            this.OnAlert?.Invoke(alert);
        }

        public void Clear(string id = _defaultId)
        {
            this.OnAlert?.Invoke(new AlertBIQ { Id = id });
        }
    }
}