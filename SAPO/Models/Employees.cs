using System;
using SAPO.Interfaces;

namespace SAPO.Models
{
    public class Employees : EmployeesInterface
    {
        public string Id;
        public string Name;
        public int FingerPrintId;
        public bool IsActive;
        public int privilege;
        
        public Employees(string id, string name, int hasFingerPrint, int privilege, bool isActive, DateTime createdAt) : base(id, name, hasFingerPrint, privilege, isActive, createdAt)
        {
            this.Id = id;
            this.Name = name;
            this.FingerPrintId = hasFingerPrint;
            this.privilege = privilege;
            this.IsActive = isActive;
        }
        
        public new string GetId()
        {
            return Id;
        }

        public new string GetName()
        {
            return Name;
        }
        
        public new int GetHasFingerPrint()
        {
            return FingerPrintId;
        }
        
        public new bool GetIsActive()
        {
            return IsActive;
        }
        public void SetName(string name)
        {
            Name = name;
        }
        
        public void SetHasFingerPrint(int hasFingerPrint)
        {
            FingerPrintId = hasFingerPrint;
        }
        
        public void SetIsActive(bool isActive)
        {
            IsActive = isActive;
        }   
        
        public void SetPrivilege(int privilege)
        {
            this.privilege = privilege;
        }
        
        public int GetPrivilege()
        {
            return privilege;
        }
        
        
        
    }
}