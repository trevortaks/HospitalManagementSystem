using System;

namespace HospitalManagementSystem.Domain.Exceptions
{
    public class DuplicateEntityException : Exception
    {
        public DuplicateEntityException(string entityName, string fieldName, string fieldValue)
            : base($"{entityName} with {fieldName} '{fieldValue}' already exists.")
        {
        }
    }
}
