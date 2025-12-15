namespace AppointmentAPI.Logging
{
    //Using an interface for logging, so that the simple console logging could be swapped out for something more robust
    public interface IApiLogger
    {
        void Log(string message);
    }
}
