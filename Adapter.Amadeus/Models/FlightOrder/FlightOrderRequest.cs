using System.Text.Json.Serialization;
using Adapter.Amadeus.Models.FlightSearch;

namespace Adapter.Amadeus.Models.FlightOrder;

/// <summary>
/// Request model for creating a flight order
/// </summary>
public class FlightOrderRequest
{
    [JsonPropertyName("data")]
    public FlightOrderData Data { get; set; } = new();
}

/// <summary>
/// Data container for flight order request
/// </summary>
public class FlightOrderData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "flight-order";

    [JsonPropertyName("flightOffers")]
    public List<FlightOffer> FlightOffers { get; set; } = new();

    [JsonPropertyName("travelers")]
    public List<TravelerInfo> Travelers { get; set; } = new();

    [JsonPropertyName("remarks")]
    public Remarks? Remarks { get; set; }

    [JsonPropertyName("ticketingAgreement")]
    public TicketingAgreement? TicketingAgreement { get; set; }

    [JsonPropertyName("contacts")]
    public List<Contact>? Contacts { get; set; }

    [JsonPropertyName("queuingOfficeId")]
    public string? QueuingOfficeId { get; set; }

    [JsonPropertyName("formOfPayment")]
    public FormOfPayment? FormOfPayment { get; set; }
}

/// <summary>
/// Traveler information for booking
/// </summary>
public class TravelerInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("dateOfBirth")]
    public string DateOfBirth { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public TravelerName Name { get; set; } = new();

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("contact")]
    public TravelerContact? Contact { get; set; }

    [JsonPropertyName("documents")]
    public List<TravelerDocument>? Documents { get; set; }
}

/// <summary>
/// Traveler name information
/// </summary>
public class TravelerName
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Traveler contact information
/// </summary>
public class TravelerContact
{
    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("phones")]
    public List<Phone>? Phones { get; set; }
}

/// <summary>
/// Phone number information
/// </summary>
public class Phone
{
    [JsonPropertyName("deviceType")]
    public string DeviceType { get; set; } = "MOBILE";

    [JsonPropertyName("countryCallingCode")]
    public string CountryCallingCode { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;
}

/// <summary>
/// Traveler document (passport, ID, etc.)
/// </summary>
public class TravelerDocument
{
    [JsonPropertyName("documentType")]
    public string DocumentType { get; set; } = "PASSPORT";

    [JsonPropertyName("birthPlace")]
    public string? BirthPlace { get; set; }

    [JsonPropertyName("issuanceLocation")]
    public string? IssuanceLocation { get; set; }

    [JsonPropertyName("issuanceDate")]
    public string? IssuanceDate { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("expiryDate")]
    public string ExpiryDate { get; set; } = string.Empty;

    [JsonPropertyName("issuanceCountry")]
    public string IssuanceCountry { get; set; } = string.Empty;

    [JsonPropertyName("validityCountry")]
    public string ValidityCountry { get; set; } = string.Empty;

    [JsonPropertyName("nationality")]
    public string Nationality { get; set; } = string.Empty;

    [JsonPropertyName("holder")]
    public bool Holder { get; set; } = true;
}

/// <summary>
/// Remarks for the order
/// </summary>
public class Remarks
{
    [JsonPropertyName("general")]
    public List<GeneralRemark>? General { get; set; }
}

/// <summary>
/// General remark
/// </summary>
public class GeneralRemark
{
    [JsonPropertyName("subType")]
    public string SubType { get; set; } = "GENERAL_MISCELLANEOUS";

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Ticketing agreement for self-service bookings
/// </summary>
public class TicketingAgreement
{
    [JsonPropertyName("option")]
    public string Option { get; set; } = "DELAY_TO_CANCEL";

    [JsonPropertyName("delay")]
    public string? Delay { get; set; }
}

/// <summary>
/// Contact information for the order
/// </summary>
public class Contact
{
    [JsonPropertyName("addresseeName")]
    public ContactName? AddresseeName { get; set; }

    [JsonPropertyName("companyName")]
    public string? CompanyName { get; set; }

    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = "STANDARD";

    [JsonPropertyName("phones")]
    public List<Phone>? Phones { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("address")]
    public ContactAddress? Address { get; set; }
}

/// <summary>
/// Contact name
/// </summary>
public class ContactName
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }
}

/// <summary>
/// Contact address
/// </summary>
public class ContactAddress
{
    [JsonPropertyName("lines")]
    public List<string>? Lines { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("cityName")]
    public string? CityName { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}

/// <summary>
/// Form of payment for the flight order
/// </summary>
public class FormOfPayment
{
    [JsonPropertyName("other")]
    public OtherFormOfPayment? Other { get; set; }

    [JsonPropertyName("creditCard")]
    public CreditCardPayment? CreditCard { get; set; }
}

/// <summary>
/// Other form of payment (cash, check, etc.)
/// </summary>
public class OtherFormOfPayment
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("voucherCode")]
    public string? VoucherCode { get; set; }

    [JsonPropertyName("embeddedPayment")]
    public EmbeddedPayment? EmbeddedPayment { get; set; }

    [JsonPropertyName("flightOfferIds")]
    public List<string>? FlightOfferIds { get; set; }
}

/// <summary>
/// Credit card payment information
/// </summary>
public class CreditCardPayment
{
    [JsonPropertyName("vendorCode")]
    public string VendorCode { get; set; } = string.Empty;

    [JsonPropertyName("cardNumber")]
    public string? CardNumber { get; set; }

    [JsonPropertyName("expiryDate")]
    public string? ExpiryDate { get; set; }

    [JsonPropertyName("cardHolderName")]
    public string? CardHolderName { get; set; }

    [JsonPropertyName("cvvCode")]
    public string? CvvCode { get; set; }

    [JsonPropertyName("billingAddress")]
    public BillingAddress? BillingAddress { get; set; }

    [JsonPropertyName("companyName")]
    public string? CompanyName { get; set; }
}

/// <summary>
/// Embedded payment information
/// </summary>
public class EmbeddedPayment
{
    [JsonPropertyName("paymentToken")]
    public string? PaymentToken { get; set; }

    [JsonPropertyName("operation")]
    public string? Operation { get; set; }
}

/// <summary>
/// Billing address for credit card payment
/// </summary>
public class BillingAddress
{
    [JsonPropertyName("lines")]
    public List<string>? Lines { get; set; }

    [JsonPropertyName("postalCode")]
    public string? PostalCode { get; set; }

    [JsonPropertyName("cityName")]
    public string? CityName { get; set; }

    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("stateCode")]
    public string? StateCode { get; set; }
}

