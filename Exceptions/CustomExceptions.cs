namespace SmartCard.Exceptions
{
    public class EmployeeNotFoundException : Exception
    {
        public EmployeeNotFoundException() : base() { }
        public EmployeeNotFoundException(string message) : base(message) { }
        public EmployeeNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class CardNotFoundException : Exception
    {
        public CardNotFoundException() : base() { }
        public CardNotFoundException(string message) : base(message) { }
        public CardNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InsufficientFuelException : Exception
    {
        public InsufficientFuelException() : base() { }
        public InsufficientFuelException(string message) : base(message) { }
        public InsufficientFuelException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidCardException : Exception
    {
        public InvalidCardException() : base() { }
        public InvalidCardException(string message) : base(message) { }
        public InvalidCardException(string message, Exception innerException) : base(message, innerException) { }
    }
}