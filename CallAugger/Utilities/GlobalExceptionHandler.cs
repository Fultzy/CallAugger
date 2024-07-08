using System;

namespace CallAugger.Utilities
{
    public static class GlobalExceptionHandler
    {
        public static void HandleException(Exception ex)
        {
            // Handle the exception here
            Console.WriteLine($"An exception occurred: {ex.Message}");
        }
    }
}
