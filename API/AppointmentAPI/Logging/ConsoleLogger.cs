namespace AppointmentAPI.Logging
{
    internal class ConsoleLogger : IApiLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
