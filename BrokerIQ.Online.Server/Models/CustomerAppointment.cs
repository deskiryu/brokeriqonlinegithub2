using System;

namespace BrokerIQ.Online.Models;

public class CustomerAppointment
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string ExternalEventId { get; set; }

    public string Title { get; set; }

    public DateTime? AppointmentDate { get; set; }

    public string Location { get; set; }
}
