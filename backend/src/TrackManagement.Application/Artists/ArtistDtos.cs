namespace TrackManagement.Application.Artists;

public record ArtistDto(int Id, string Name, string Email, string Country);

public record CreateArtistRequest(string Name, string Email, string Country);
