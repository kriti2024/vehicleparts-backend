

namespace VehicleParts.Application.Exceptions;

//Invalid login
//Wrong password
//Account disabled
//No token
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}