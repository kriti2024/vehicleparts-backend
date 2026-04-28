using System.Collections;

namespace VehicleParts.Application.Exceptions;

//Staff tries admin action
//Access denied
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}