using System.Text.Json.Serialization;

namespace Adapter.Amadeus.Models.FlightSearch;

/// <summary>
/// Response model for Flight Offers Price endpoint
/// Contains confirmed pricing with detailed fare rules
/// </summary>
public class FlightOffersPricingResponse
{
    [JsonPropertyName("data")]
    public FlightOffersPricingResponseData? Data { get; set; }

    [JsonPropertyName("warnings")]
    public List<Warning>? Warnings { get; set; }
}

/// <summary>
/// Data container for flight offers pricing response
/// </summary>
public class FlightOffersPricingResponseData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("flightOffers")]
    public List<PricedFlightOffer> FlightOffers { get; set; } = new();

    [JsonPropertyName("bookingRequirements")]
    public BookingRequirements? BookingRequirements { get; set; }
}

/// <summary>
/// Flight offer with confirmed pricing and detailed fare rules
/// </summary>
public class PricedFlightOffer : FlightOffer
{
    [JsonPropertyName("fareRules")]
    public List<FareRule>? FareRules { get; set; }
}

/// <summary>
/// Detailed fare rule information
/// </summary>
public class FareRule
{
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("categoryCode")]
    public string? CategoryCode { get; set; }

    [JsonPropertyName("tariffNumber")]
    public string? TariffNumber { get; set; }

    [JsonPropertyName("fareBasis")]
    public string? FareBasis { get; set; }

    [JsonPropertyName("rules")]
    public List<FareRuleDetail>? Rules { get; set; }
}

/// <summary>
/// Detailed fare rule information
/// </summary>
public class FareRuleDetail
{
    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("categoryCode")]
    public string? CategoryCode { get; set; }

    [JsonPropertyName("freeText")]
    public string? FreeText { get; set; }

    [JsonPropertyName("structuredFreeText")]
    public List<string>? StructuredFreeText { get; set; }
}

/// <summary>
/// Booking requirements for the flight offer
/// </summary>
public class BookingRequirements
{
    [JsonPropertyName("emailAddressRequired")]
    public bool EmailAddressRequired { get; set; }

    [JsonPropertyName("mobilePhoneNumberRequired")]
    public bool MobilePhoneNumberRequired { get; set; }

    [JsonPropertyName("travelerRequirements")]
    public List<TravelerRequirement>? TravelerRequirements { get; set; }
}

/// <summary>
/// Requirements for a specific traveler
/// </summary>
public class TravelerRequirement
{
    [JsonPropertyName("travelerId")]
    public string TravelerId { get; set; } = string.Empty;

    [JsonPropertyName("genderRequired")]
    public bool GenderRequired { get; set; }

    [JsonPropertyName("documentRequired")]
    public bool DocumentRequired { get; set; }

    [JsonPropertyName("documentIssuanceCityRequired")]
    public bool DocumentIssuanceCityRequired { get; set; }

    [JsonPropertyName("dateOfBirthRequired")]
    public bool DateOfBirthRequired { get; set; }

    [JsonPropertyName("redressRequiredIfAny")]
    public bool RedressRequiredIfAny { get; set; }

    [JsonPropertyName("airFranceDiscountRequired")]
    public bool AirFranceDiscountRequired { get; set; }

    [JsonPropertyName("spanishResidentDiscountRequired")]
    public bool SpanishResidentDiscountRequired { get; set; }
}

/// <summary>
/// Warning message from the API
/// </summary>
public class Warning
{
    [JsonPropertyName("status")]
    public int Status { get; set; }

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}

