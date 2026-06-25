namespace Appegy.Ump
{
    public enum UmpConsentStatus
    {
        Unknown = 0,
        NotRequired = 1,
        Required = 2,
        Obtained = 3
    }

    public enum UmpDebugGeography
    {
        Disabled = 0,
        Eea = 1,
        RegulatedUsState = 3,
        Other = 4
    }

    public enum UmpErrorCode
    {
        Network = 0,
        FormUnavailable = 1,
        FormLoadFailed = 2,
        Internal = 3
    }

    public readonly struct UmpError
    {
        public UmpErrorCode Code { get; }
        public int NativeCode { get; }
        public string Message { get; }

        public UmpError(UmpErrorCode code, int nativeCode, string message)
        {
            Code = code;
            NativeCode = nativeCode;
            Message = message ?? string.Empty;
        }

        public override string ToString()
        {
            return $"{Code} (native {NativeCode}): {Message}";
        }
    }

    public readonly struct UmpResult
    {
        public UmpConsentStatus Status { get; }
        public UmpError? Error { get; }
        public bool IsSuccess => Error == null;

        public UmpResult(UmpConsentStatus status, UmpError? error = null)
        {
            Status = status;
            Error = error;
        }
    }

    public struct UmpInitializeParameters
    {
        public UmpDebugGeography DebugGeography;
        public string TestDeviceHashedId;
    }
}
