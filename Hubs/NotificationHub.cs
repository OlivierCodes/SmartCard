using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SmartCard.Hubs
{
    [Authorize] // Exiger l'authentification pour le hub
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Vérifier si l'utilisateur est authentifié (web ou mobile)
            await base.OnConnectedAsync();
        }

        public async Task SendNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        public async Task SendTransactionUpdate(string cardNumber, decimal amount)
        {
            var update = new MobileTransactionUpdate
            {
                CardNumber = cardNumber,
                Amount = amount,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("ReceiveTransactionUpdate", update);
        }

        public async Task SendConsumptionUpdate(int employeeId, string employeeName, decimal amount)
        {
            var update = new MobileConsumptionUpdate
            {
                EmployeeId = employeeId,
                EmployeeName = employeeName,
                Amount = amount,
                Timestamp = DateTime.UtcNow
            };

            await Clients.All.SendAsync("ReceiveConsumptionUpdate", update);
        }

        // Nouveau: Envoi de mises à jour spécifiques pour les pompistes mobiles
        public async Task SendPumpAttendantUpdate(MobilePumpUpdate update)
        {
            await Clients.All.SendAsync("ReceivePumpAttendantUpdate", update);
        }
    }

    public class MobileTransactionUpdate
    {
        public string CardNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MobileConsumptionUpdate
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class MobilePumpUpdate
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public double Amount { get; set; }
        public double RemainingQuota { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; } = string.Empty; // Success, Error, etc.
    }
}