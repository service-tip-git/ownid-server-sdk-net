using System;

namespace OwnID.Extensibility.Exceptions
{
    public sealed class OwnIdException : Exception
    {
        public OwnIdException(ErrorType errorType, bool finishFlow = true) : this(errorType,
            GetErrorTypeLocalizationKey(errorType), finishFlow)
        {
        }

        public OwnIdException(ErrorType errorType, string message, bool shouldStopFlow = true) : base(message)
        {
            ErrorType = errorType;
            ShouldStopFlow = shouldStopFlow;
        }

        public ErrorType ErrorType { get; }

        public bool ShouldStopFlow { get; }

        private static string GetErrorTypeLocalizationKey(ErrorType errorType)
        {
            switch (errorType)
            {
                case ErrorType.UserAlreadyExists:
                    return "Error_PhoneAlreadyConnected";
                case ErrorType.UserNotFound:
                    return "User was not found"; // TODO: add to resources
                case ErrorType.RequiresBiometricInput:
                    break;
                case ErrorType.RecoveryTokenExpired:
                    break;
            }

            return string.Empty;
        }
    }
}