namespace HospitalManagementSystem.Application.Common.Exceptions;

public class DuplicateEntityException : ConflictException
{
    public DuplicateEntityException(string entityName, string fieldName, string fieldValue)
        : base($"Entity \"{entityName}\" with {fieldName} \"{fieldValue}\" already exists.")
    {
    }
}
