namespace Tiki;

/// <summary>
/// Base exception for all Tiki errors.
/// </summary>
public class TikiException : Exception
{
    public TikiException() { }
    public TikiException(string message) : base(message) { }
    public TikiException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when a parser encounters an error while parsing a document.
/// </summary>
public class ParseException : TikiException
{
    public ParseException() { }
    public ParseException(string message) : base(message) { }
    public ParseException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when MIME type detection fails.
/// </summary>
public class DetectionException : TikiException
{
    public DetectionException() { }
    public DetectionException(string message) : base(message) { }
    public DetectionException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when a document is encrypted or password-protected.
/// </summary>
public class EncryptedDocumentException : TikiException
{
    public EncryptedDocumentException() { }
    public EncryptedDocumentException(string message) : base(message) { }
    public EncryptedDocumentException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when no parser is available for the detected format.
/// </summary>
public class UnsupportedFormatException : TikiException
{
    public UnsupportedFormatException() { }
    public UnsupportedFormatException(string message) : base(message) { }
    public UnsupportedFormatException(string message, Exception innerException) : base(message, innerException) { }
}
