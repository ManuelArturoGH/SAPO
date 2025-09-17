using System;

namespace BioMetrixCore
{
    public class EmployeesInterface
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Hora { get; set; }
        public string Status { get; set; }
        public string WorkedHours { get; set; }

        public EmployeesInterface(string id, string name, string date, string hora, string status, string workedHours)
        {
            Id = id;
            Name = name;
            Date = date;
            Hora = hora;
            Status = status;
            WorkedHours = workedHours;
        }
    }
}