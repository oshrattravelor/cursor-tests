using System.Text;
using System.Text.Json;
using Adapter.Amadeus.Configuration;
using Adapter.Amadeus.Models.FlightOrder;
using Microsoft.Extensions.Options;

namespace Adapter.Amadeus.Services;

public class AmadeusFlightOrderService : IAmadeusFlightOrderService
{
    private readonly HttpClient _httpClient;
    private readonly IAmadeusAuthService _authService;
    private readonly AmadeusSettings _settings;
    private readonly IHttpRequestLogger _requestLogger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AmadeusFlightOrderService(
        HttpClient httpClient,
        IAmadeusAuthService authService,
        IOptions<AmadeusSettings> settings,
        IHttpRequestLogger requestLogger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _settings = settings.Value;
        _requestLogger = requestLogger;

        var baseUrl = _settings.IsProduction
            ? "https://api.amadeus.com"
            : "https://test.travel.api.amadeus.com";

        _httpClient.BaseAddress = new Uri(baseUrl);

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<FlightOrderResponse> CreateFlightOrderAsync(
        FlightOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request?.Data == null)
        {
            throw new ArgumentException("Request data is required", nameof(request));
        }

        if (request.Data.FlightOffers == null || request.Data.FlightOffers.Count == 0)
        {
            throw new ArgumentException("At least one flight offer is required", nameof(request));
        }

        if (request.Data.Travelers == null || request.Data.Travelers.Count == 0)
        {
            throw new ArgumentException("At least one traveler is required", nameof(request));
        }

        // Get access token
        var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);

        // Build the request body
        var requestBody = new
        {
            data = new
            {
                type = request.Data.Type,
                flightOffers = request.Data.FlightOffers,
                travelers = request.Data.Travelers.Select(t => new
                {
                    id = t.Id,
                    dateOfBirth = t.DateOfBirth,
                    name = new
                    {
                        firstName = t.Name.FirstName,
                        lastName = t.Name.LastName
                    },
                    gender = t.Gender,
                    contact = t.Contact != null ? new
                    {
                        emailAddress = t.Contact.EmailAddress,
                        phones = t.Contact.Phones?.Select(p => new
                        {
                            deviceType = p.DeviceType,
                            countryCallingCode = p.CountryCallingCode,
                            number = p.Number
                        }).ToArray()
                    } : null,
                    documents = t.Documents?.Select(d => new
                    {
                        documentType = d.DocumentType,
                        birthPlace = d.BirthPlace,
                        issuanceLocation = d.IssuanceLocation,
                        issuanceDate = d.IssuanceDate,
                        number = d.Number,
                        expiryDate = d.ExpiryDate,
                        issuanceCountry = d.IssuanceCountry,
                        validityCountry = d.ValidityCountry,
                        nationality = d.Nationality,
                        holder = d.Holder
                    }).ToArray()
                }).ToArray(),
                remarks = request.Data.Remarks != null ? new
                {
                    general = request.Data.Remarks.General?.Select(r => new
                    {
                        subType = r.SubType,
                        text = r.Text
                    }).ToArray()
                } : null,
                ticketingAgreement = request.Data.TicketingAgreement != null ? new
                {
                    option = request.Data.TicketingAgreement.Option,
                    delay = request.Data.TicketingAgreement.Delay
                } : null,
                contacts = request.Data.Contacts?.Select(c => new
                {
                    addresseeName = c.AddresseeName != null ? new
                    {
                        firstName = c.AddresseeName.FirstName,
                        lastName = c.AddresseeName.LastName
                    } : null,
                    companyName = c.CompanyName,
                    purpose = c.Purpose,
                    phones = c.Phones?.Select(p => new
                    {
                        deviceType = p.DeviceType,
                        countryCallingCode = p.CountryCallingCode,
                        number = p.Number
                    }).ToArray(),
                    emailAddress = c.EmailAddress,
                    address = c.Address != null ? new
                    {
                        lines = c.Address.Lines,
                        postalCode = c.Address.PostalCode,
                        cityName = c.Address.CityName,
                        countryCode = c.Address.CountryCode
                    } : null
                }).ToArray(),
                queuingOfficeId = request.Data.QueuingOfficeId,
                formOfPayment = request.Data.FormOfPayment != null ? new
                {
                    other = request.Data.FormOfPayment.Other != null ? new
                    {
                        method = request.Data.FormOfPayment.Other.Method,
                        voucherCode = request.Data.FormOfPayment.Other.VoucherCode,
                        embeddedPayment = request.Data.FormOfPayment.Other.EmbeddedPayment != null ? new
                        {
                            paymentToken = request.Data.FormOfPayment.Other.EmbeddedPayment.PaymentToken,
                            operation = request.Data.FormOfPayment.Other.EmbeddedPayment.Operation
                        } : null,
                        flightOfferIds = request.Data.FormOfPayment.Other.FlightOfferIds
                    } : null,
                    creditCard = request.Data.FormOfPayment.CreditCard != null ? new
                    {
                        vendorCode = request.Data.FormOfPayment.CreditCard.VendorCode,
                        cardNumber = request.Data.FormOfPayment.CreditCard.CardNumber,
                        expiryDate = request.Data.FormOfPayment.CreditCard.ExpiryDate,
                        cardHolderName = request.Data.FormOfPayment.CreditCard.CardHolderName,
                        cvvCode = request.Data.FormOfPayment.CreditCard.CvvCode,
                        billingAddress = request.Data.FormOfPayment.CreditCard.BillingAddress != null ? new
                        {
                            lines = request.Data.FormOfPayment.CreditCard.BillingAddress.Lines,
                            postalCode = request.Data.FormOfPayment.CreditCard.BillingAddress.PostalCode,
                            cityName = request.Data.FormOfPayment.CreditCard.BillingAddress.CityName,
                            countryCode = request.Data.FormOfPayment.CreditCard.BillingAddress.CountryCode,
                            stateCode = request.Data.FormOfPayment.CreditCard.BillingAddress.StateCode
                        } : null,
                        companyName = request.Data.FormOfPayment.CreditCard.CompanyName
                    } : null
                } : null
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v2/booking/flight-orders")
        {
            Content = content
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
        httpRequest.Headers.Add("Ama-Client-Ref", $"TRAVELOR BOOKING ENGINE-PDT-{DateTime.UtcNow.ToString()}");

        // Send request
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        // Log request and response
        var headers = new Dictionary<string, string>();
        if (httpRequest.Headers != null)
        {
            foreach (var header in httpRequest.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }
        }

        await _requestLogger.LogRequestResponseAsync(
            "FlightOrder",
            "/v2/booking/flight-orders",
            "POST",
            jsonContent,
            headers,
            responseContent,
            (int)response.StatusCode);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Flight order creation failed. Status: {response.StatusCode}, Error: {responseContent}");
        }

        // Parse response
        var orderResponse = JsonSerializer.Deserialize<FlightOrderResponse>(
            responseContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (orderResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize flight order response");
        }

        return orderResponse;
    }
}

