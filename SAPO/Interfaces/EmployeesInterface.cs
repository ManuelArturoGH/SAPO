using System;

namespace SAPO.Interfaces
{
    public class EmployeesInterface 
    {
        private readonly string Id;
        private readonly string Name;
        private readonly int FingerPrintId;
        private readonly bool IsActive;
        private readonly int privilege;
        public EmployeesInterface(string id, string name, int hasFingerPrint, int privilege, bool isActive, DateTime createdAt)
        {
            Id = id;
            Name = name;
            FingerPrintId = hasFingerPrint;
            this.privilege = privilege;
            IsActive = isActive;
        }
        
        public string GetId()
        {
            return Id;
        }
        public string GetName()
        {
            return Name;
        }

        public int GetHasFingerPrint()
        {
            return FingerPrintId;
        }
        public bool GetIsActive()
        {
            return IsActive;
        }
        
    }
}