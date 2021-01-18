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
                    return "Error_UserNotFound";
                case ErrorType.RequiresBiometricInput:
                    return "Error_RequiresBiometricInput";
                case ErrorType.RecoveryTokenExpired:
                    return "Error_RecoveryTokenExpired";
            }

            return string.Empty;
        }
    }
}