using TrackManagement.Application.Artists;
using TrackManagement.Application.Auth;
using TrackManagement.Application.Tracks;

namespace TrackManagement.Tests.Validation;

public class CreateTrackRequestValidatorTests
{
    private readonly CreateTrackRequestValidator _validator = new();

    private static CreateTrackRequest Valid(string isrc = "EG-TKW-24-00001") =>
        new(ArtistId: 1, Title: "Leil", Isrc: isrc, ReleaseDate: new DateOnly(2024, 3, 15), Genre: "Arabic Pop");

    [Theory]
    [InlineData("EG-TKW-24-00001")] // hyphenated
    [InlineData("EGTKW2400001")]    // compact
    [InlineData("sa1ab2500042")]    // lower case, digits in registrant code
    public void Accepts_valid_isrc(string isrc)
    {
        Assert.True(_validator.Validate(Valid(isrc)).IsValid);
    }

    [Theory]
    [InlineData("ABC")]                // too short
    [InlineData("EG-TKW-24-0001")]     // 11 characters
    [InlineData("12-TKW-24-00001")]    // country code must be letters
    [InlineData("EG-TKW-AB-00001")]    // year must be digits
    [InlineData("EG-TK!-24-00001")]    // registrant must be letters/digits
    public void Rejects_invalid_isrc_with_a_clear_message(string isrc)
    {
        var result = _validator.Validate(Valid(isrc));

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(CreateTrackRequest.Isrc), error.PropertyName);
        Assert.Contains("CC-XXX-YY-NNNNN", error.ErrorMessage);
    }

    [Fact]
    public void Reports_every_missing_field()
    {
        var result = _validator.Validate(new CreateTrackRequest(0, "", "", null, ""));

        var fields = result.Errors.Select(e => e.PropertyName).ToHashSet();
        Assert.Equal(
            new HashSet<string> { "ArtistId", "Title", "Isrc", "ReleaseDate", "Genre" },
            fields);
    }
}

public class CreateArtistRequestValidatorTests
{
    private readonly CreateArtistRequestValidator _validator = new();

    [Theory]
    [InlineData("EG", true)]
    [InlineData("sa", true)]
    [InlineData("EGY", false)]
    [InlineData("E1", false)]
    [InlineData("", false)]
    public void Country_must_be_a_two_letter_code(string country, bool expectedValid)
    {
        var result = _validator.Validate(new CreateArtistRequest("Nour", "nour@example.com", country));

        Assert.Equal(expectedValid, result.IsValid);
    }

    [Fact]
    public void Rejects_invalid_email()
    {
        var result = _validator.Validate(new CreateArtistRequest("Nour", "not-an-email", "EG"));

        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }
}

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Theory]
    [InlineData("Passw0rd", true)]
    [InlineData("short1", false)]       // fewer than 8 characters
    [InlineData("onlyletters", false)]  // no number
    [InlineData("1234567890", false)]   // no letter
    public void Password_rules(string password, bool expectedValid)
    {
        var result = _validator.Validate(new RegisterRequest("Demo User", "demo@example.com", password));

        Assert.Equal(expectedValid, result.IsValid);
    }
}

public class UpdateRequestValidatorTests
{
    [Fact]
    public void Track_update_uses_the_same_rules_as_create()
    {
        var result = new UpdateTrackRequestValidator().Validate(
            new UpdateTrackRequest(ArtistId: 0, Title: "", Isrc: "BAD", ReleaseDate: null, Genre: ""));

        Assert.Equal(
            new[] { "ArtistId", "Genre", "Isrc", "ReleaseDate", "Title" },
            result.Errors.Select(e => e.PropertyName).OrderBy(p => p));
    }

    [Fact]
    public void Artist_update_uses_the_same_rules_as_create()
    {
        var result = new UpdateArtistRequestValidator().Validate(
            new UpdateArtistRequest(Name: "", Email: "not-an-email", Country: "EGY"));

        Assert.Equal(
            new[] { "Country", "Email", "Name" },
            result.Errors.Select(e => e.PropertyName).OrderBy(p => p));
    }
}
