using System;

namespace BrokerIQ.Online.Models;

public class CustomerAppointment
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string CustomerName { get; set; }

    public string ExternalEventId { get; set; }

    public string Title { get; set; }

    public DateTime? AppointmentDate { get; set; }

    public string Location { get; set; }

    public string ContactName { get; set; }

    public string CustomerEmail { get; set; }

    public bool Expired { get => AppointmentDate.HasValue && AppointmentDate < DateTime.UtcNow; }

    public string LabelColour { get => Expired ? "red" : "black"; }
}
