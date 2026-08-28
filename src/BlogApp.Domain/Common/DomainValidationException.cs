namespace BlogApp.Domain.Common;

/// <summary>Raised when an operation would leave an entity in an invalid state.</summary>
public sealed class DomainValidationException(string message) : Exception(message);
