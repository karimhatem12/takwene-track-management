using FluentValidation;
using FluentValidation.Results;

namespace TrackManagement.Application.Common.Validation;

internal static class ValidationExtensions
{
    /// <summary>
    /// Throws a FluentValidation <see cref="ValidationException"/> for a single field.
    /// Used for checks that need the database (e.g. "artist does not exist"),
    /// so they reach the client in the same 400 format as normal validation errors.
    /// </summary>
    public static ValidationException FieldError(string propertyName, string message) =>
        new(new[] { new ValidationFailure(propertyName, message) });
}
