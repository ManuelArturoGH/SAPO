using System;
using SAPO.Interfaces;

namespace SAPO.Models
{
    public class Employees : EmployeesInterface
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int FingerPrintId { get; set; }
        public bool IsActive { get; set; }
        public int Privilege { get; set; }

        public Employees() : base("", "", 0, 0, false, DateTime.UtcNow) { }

        public Employees(string id, string name, int hasFingerPrint, int privilege, bool isActive, DateTime createdAt)
            : base(id, name, hasFingerPrint, privilege, isActive, createdAt)
        {
            Id = id;
            Name = name;
            FingerPrintId = hasFingerPrint;
            Privilege = privilege;
            IsActive = isActive;
        }

        public new string GetId() => Id;
        public new string GetName() => Name;
        public new int GetHasFingerPrint() => FingerPrintId;
        public new bool GetIsActive() => IsActive;
        public int GetPrivilege() => Privilege;
    }
}