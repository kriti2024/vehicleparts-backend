
namespace VehicleParts.Application.Exceptions;

//Customer not found
//Sale not found
//Part not found
//User not found
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}