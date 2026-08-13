namespace Toast;

public class ToastException(ErrorValue error) : InvalidOperationException(error.ToString())
{
    public ErrorValue Error { get; } = error;

    public ToastException(
        string errorType,
        string message,
        Location? location = null,
        ToastValue? cause = null
    )
        : this(new ErrorValue(errorType, message, location, cause)) { }
}
