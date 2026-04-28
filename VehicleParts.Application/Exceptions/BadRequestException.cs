namespace VehicleParts.Application.Exceptions;

//Invalid role
//Duplicate email
//Wrong input
//Stock insufficient
public class BadRequestException : Exception
{
	public BadRequestException(string message) : base(message)
	{
	}
}