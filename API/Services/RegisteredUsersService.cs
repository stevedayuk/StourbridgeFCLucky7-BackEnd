using System.Text;
using brevo_csharp.Api;
using brevo_csharp.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.Configuration;
using StourbridgeFc.Lucky7.Api.DataTransferObjects;
using StourbridgeFc.Lucky7.Data;
using StourbridgeFc.Lucky7.Data.Models;
using Task = System.Threading.Tasks.Task;

namespace StourbridgeFc.Lucky7.Api.Services;

public class RegisteredUsersService
{
    private readonly IConfiguration _configuration;
    private readonly Lucky7Context _dataContext;
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public RegisteredUsersService(IConfiguration configuration, Lucky7Context dataContext, HttpClient httpClient,
        IWebHostEnvironment webHostEnvironment)
    {
        _configuration = configuration;
        _dataContext = dataContext;
        _httpClient = httpClient;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<bool> ConfirmEntryAsync(string verificationCode)
    {
        RegisteredUser? registeredUser =
            await _dataContext.RegisteredUsers.FirstOrDefaultAsync(u =>
                u.VerificationCode == verificationCode && u.ConfirmedDate == null);

        if (registeredUser is null)
            return false;

        registeredUser.ConfirmedDate = DateTime.UtcNow;
        await _dataContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsUserEntryConfirmedAsync(string verificationCode)
    {
        bool isUserEntryConfirmed =
            await _dataContext.RegisteredUsers.AnyAsync(u =>
                u.VerificationCode == verificationCode && u.ConfirmedDate != null);
        return isUserEntryConfirmed;
    }
    
    public async Task<bool> IsUserVerificationCodeValidAsync(string verificationCode)
    {
        bool isUserVerificationCodeValid =
            await _dataContext.RegisteredUsers.AnyAsync(u => u.VerificationCode == verificationCode && u.VerifiedDate != null);
        return isUserVerificationCodeValid;
    }
    
    public async Task<bool> IsUserVerifiedAsync(string verificationCode)
    {
        bool isUserVerified =
            await _dataContext.RegisteredUsers.AnyAsync(u => u.VerificationCode == verificationCode && u.VerifiedDate != null);
        return isUserVerified;
    }

    public async Task<bool> RegisterUserAsync(RegisterUserDto registerUser)
    {
        bool isTurnstileTokenVerified = await VerifyTurnstileTokenAsync(registerUser.TurnstileToken);

        if (isTurnstileTokenVerified is false)
            return false;

        bool isUserRegistered = await IsUserRegisteredAsync(registerUser);

        if (isUserRegistered)
            return false;

        string verificationCode = GenerateVerificationCode();
        await SaveUserInDatabaseAsync(registerUser, verificationCode);
        await SendUserVerificationEmailAsync(registerUser, verificationCode);
        return true;
    }

    public async Task<bool> SendEntryConfirmedEmailsAsync(string verificationCode)
    {
        RegisteredUserDto? registeredUser = await _dataContext.RegisteredUsers.Where(p => p.VerificationCode == verificationCode)
            .Select(p => new RegisteredUserDto
            {
                Name = p.Name,
                AddressLine1 = p.AddressLine1,
                AddressLine2 = p.AddressLine2,
                Town = p.Town,
                County = p.County,
                Postcode = p.Postcode,
                Email = p.Email,
                Telephone = p.Telephone
            }).FirstOrDefaultAsync();

        if (registeredUser is null)
            return false;

        ConfirmedUserDto confirmedUser = new()
        {
            Name = registeredUser.Name,
            Email = registeredUser.Email
        };

        await SendEntryConfirmedEmailAsync(confirmedUser);
        await SendInternalEntryConfirmedEmailAsync(registeredUser);
        return true;
    }

    public async Task<bool> SendUserVerifiedEmailAsync(string verificationCode)
    {
        VerifiedUserDto? verifiedUser = await _dataContext.RegisteredUsers
            .Where(p => p.VerificationCode == verificationCode)
            .Select(p => new VerifiedUserDto
            {
                Name = p.Name,
                Email = p.Email,
                VerificationCode = p.VerificationCode!
            }).FirstOrDefaultAsync();

        if (verifiedUser is null)
            return false;
        
        await SendUserVerifiedEmailAsync(verifiedUser);
        return true;
    }

    public async Task<bool> VerifyUserAsync(string code)
    {
        RegisteredUser? userToVerify =
            await _dataContext.RegisteredUsers.FirstOrDefaultAsync(r =>
                r.VerificationCode == code);

        if (userToVerify is null)
            return false;

        if (userToVerify.VerifiedDate is not null)
            return true;

        userToVerify.VerifiedDate = DateTime.UtcNow;
        await _dataContext.SaveChangesAsync();
        return true;
    }

    private async Task<bool> IsUserRegisteredAsync(RegisterUserDto registerUser)
    {
        bool isUserRegistered = await _dataContext.RegisteredUsers.AnyAsync(r => r.Email == registerUser.Email);
        return isUserRegistered;
    }

    private string GenerateVerificationCode()
    {
        // Generate random string which will be used as part of the email verification step.
        string verificationCode = Guid.NewGuid().ToString("N").Substring(0, 20);

        return verificationCode;
    }

    private async Task<bool> SaveUserInDatabaseAsync(RegisterUserDto registerUser, string verificationCode)
    {
        RegisteredUser newRegisteredUser = new()
        {
            Name = registerUser.Name,
            AddressLine1 = registerUser.AddressLine1,
            AddressLine2 = registerUser.AddressLine2,
            Town = registerUser.Town,
            County = registerUser.County,
            Postcode = registerUser.Postcode,
            Email = registerUser.Email,
            Telephone = registerUser.Telephone,
            VerificationCode = verificationCode
        };

        _dataContext.RegisteredUsers.Add(newRegisteredUser);
        int rowsAffected = await _dataContext.SaveChangesAsync();
        return rowsAffected == 1;
    }

    private async Task<bool> SendEntryConfirmedEmailAsync(ConfirmedUserDto confirmedUser)
    {
        string portalUrl = _configuration["PortalUrls:SfcLucky7"] ??
                           throw new InvalidConfigurationException(
                               "PortalUrls:SfcLucky7 value not found in configuration.");
        
        var apiInstance = new TransactionalEmailsApi();

        string emailTextContent = """
                                   Thank you for confirming your entry for Stourbridge FC's Lucky 7 draw. We've noted that you've set up your £5 monthly standing order and made your first payment.
                                   
                                   As soon as your first payment has been received, we'll email your monthly draw number. From then on, for as long as your standing order is active, you'll be entered in every monthly draw with a chance to win one of our seven prizes — from £50 up to £250!
                                   """;

        string verifyEmailHtmlFileLocation =
            Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates", "entry-confirmed.html");
        string emailHtmlContent = await File.ReadAllTextAsync(verifyEmailHtmlFileLocation);

        var sendSmtpEmail = new SendSmtpEmail
        {
            Sender = new("Stourbridge FC | Lucky 7", "noreply@stourbridgefclucky7.com"),
            To = [new SendSmtpEmailTo(confirmedUser.Email, confirmedUser.Name)],
            Subject = "Thanks for confirming your Lucky 7 entry",
            TextContent = emailTextContent,
            HtmlContent = emailHtmlContent
        };

        CreateSmtpEmail result = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
        return string.IsNullOrWhiteSpace(result.MessageId) is false;
    }

    private async Task<bool> SendInternalEntryConfirmedEmailAsync(RegisteredUserDto registeredUser)
    {
        string lucky7EmailAddress = _configuration["EmailAddresses:Lucky7"] ??
                                     throw new InvalidConfigurationException(
                                         "EmailAddresses:Lucky7 value not found in configuration.");
        
        var apiInstance = new TransactionalEmailsApi();
        
        StringBuilder addressStringBuilder = new StringBuilder();
        if (string.IsNullOrWhiteSpace(registeredUser.AddressLine1) is false)
        {
            addressStringBuilder.Append(registeredUser.AddressLine1);
            addressStringBuilder.Append(", ");
        }
        if (string.IsNullOrWhiteSpace(registeredUser.AddressLine2) is false)
        {
            addressStringBuilder.Append(registeredUser.AddressLine2);
            addressStringBuilder.Append(", ");
        }
        if (string.IsNullOrWhiteSpace(registeredUser.Town) is false)
        {
            addressStringBuilder.Append(registeredUser.Town);
            addressStringBuilder.Append(", ");
        }
        if (string.IsNullOrWhiteSpace(registeredUser.County) is false)
        {
            addressStringBuilder.Append(registeredUser.County);
            addressStringBuilder.Append(", ");
        }
        if (string.IsNullOrWhiteSpace(registeredUser.Postcode) is false)
        {
            addressStringBuilder.Append(registeredUser.Postcode);
            addressStringBuilder.Append(", ");
        }
        
        if (addressStringBuilder.Length > 0)
        {
            addressStringBuilder.Remove(addressStringBuilder.Length - 2, 2);
        }

        string telephone = string.IsNullOrWhiteSpace(registeredUser.Telephone) 
            ? "Not provided" : registeredUser.Telephone;

        string emailTextContent = $"""
                                   A new member has signed up for the Lucky 7 draw and confirmed they've setup their standing order. Here are the details they registered with:

                                   Name: {registeredUser.Name}
                                   Address: {addressStringBuilder}
                                   Email: {registeredUser.Email}
                                   Telephone: {telephone}
                                   """;

        string newRegistrationHtmlFileLocation =
            Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates", "entry-confirmed-internal.html");
        string emailHtmlContent = await File.ReadAllTextAsync(newRegistrationHtmlFileLocation);
        
        // Replace placeholders with actual values
        emailHtmlContent = emailHtmlContent
            .Replace("{{Name}}", registeredUser.Name)
            .Replace("{{Address}}", addressStringBuilder.ToString())
            .Replace("{{Email}}", registeredUser.Email)
            .Replace("{{Telephone}}", telephone);
        
        var sendSmtpEmail = new SendSmtpEmail
        {
            Sender = new("Stourbridge FC | Lucky 7", "noreply@stourbridgefclucky7.com"),
            To = [new SendSmtpEmailTo(lucky7EmailAddress, lucky7EmailAddress)],
            Subject = "New member registered",
            TextContent = emailTextContent,
            HtmlContent = emailHtmlContent
        };

        CreateSmtpEmail result = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
        return string.IsNullOrWhiteSpace(result.MessageId) is false;
    }
    
    private async Task<bool> SendUserVerifiedEmailAsync(VerifiedUserDto verifiedUser)
    {
        string portalUrl = _configuration["PortalUrls:SfcLucky7"] ??
                           throw new InvalidConfigurationException(
                               "PortalUrls:SfcLucky7 value not found in configuration.");
        
        var apiInstance = new TransactionalEmailsApi();

        string emailTextContent = $"""
                                   Thank you for registering for Stourbridge FC's Lucky 7 draw - your email address has been confirmed. That's step 2 of the sign-up complete!
                                   
                                   The final step is setting up your &pound;5 monthly standing order. 
                                   We've prepared your own page with details on how to do this.
                                   
                                   Click below to get started.

                                   {portalUrl}/setup-standing-order?code={verifiedUser.VerificationCode}
                                   """;

        string verifyEmailHtmlFileLocation =
            Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates", "email-verified.html");
        string emailHtmlContent = await File.ReadAllTextAsync(verifyEmailHtmlFileLocation);
        
        // Replace placeholders with actual values
        emailHtmlContent = emailHtmlContent
            .Replace("{{PortalUrl}}", portalUrl)
            .Replace("{{VerificationCode}}", verifiedUser.VerificationCode);

        var sendSmtpEmail = new SendSmtpEmail
        {
            Sender = new("Stourbridge FC | Lucky 7", "noreply@stourbridgefclucky7.com"),
            To = [new SendSmtpEmailTo(verifiedUser.Email, verifiedUser.Name)],
            Subject = "You're verified! Make your first payment and setup your standing order to complete your Lucky 7 entry.",
            TextContent = emailTextContent,
            HtmlContent = emailHtmlContent
        };

        CreateSmtpEmail result = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
        return string.IsNullOrWhiteSpace(result.MessageId) is false;
    }

    private async Task<bool> SendUserVerificationEmailAsync(RegisterUserDto registerUser, string verificationCode)
    {
        string portalUrl = _configuration["PortalUrls:SfcLucky7"] ??
                           throw new InvalidConfigurationException(
                               "PortalUrls:SfcLucky7 value not found in configuration.");
        
        var apiInstance = new TransactionalEmailsApi();

        string emailTextContent = $"""
                                   Thank you for registering for Stourbridge FC's Lucky 7 draw. You're now even closer to supporting your club and getting your chance to win one of our seven monthly prizes!

                                   So we know you're a real person, click the link below to verify your email address.

                                   {portalUrl}/signup/verify-email?code={verificationCode}
                                   """;

        string verifyEmailHtmlFileLocation =
            Path.Combine(_webHostEnvironment.ContentRootPath, "EmailTemplates", "verify-email.html");
        string emailHtmlContent = await File.ReadAllTextAsync(verifyEmailHtmlFileLocation);
        
        // Replace placeholders with actual values
        emailHtmlContent = emailHtmlContent
            .Replace("{{PortalUrl}}", portalUrl)
            .Replace("{{VerificationCode}}", verificationCode);

        var sendSmtpEmail = new SendSmtpEmail
        {
            Sender = new("Stourbridge FC | Lucky 7", "noreply@stourbridgefclucky7.com"),
            To = [new SendSmtpEmailTo(registerUser.Email, registerUser.Name)],
            Subject = "Verify your email",
            TextContent = emailTextContent,
            HtmlContent = emailHtmlContent
        };

        CreateSmtpEmail result = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
        return string.IsNullOrWhiteSpace(result.MessageId) is false;
    }

    private async Task<bool> VerifyTurnstileTokenAsync(string token)
    {
        var cloudflareTurnstileSecret = _configuration["Cloudflare:Turnstile:Secret"] ??
                                        throw new InvalidConfigurationException(
                                            "Cloudflare:Turnstile:Secret value not found in configuration.");

        var parameters = new Dictionary<string, string>
        {
            { "secret", cloudflareTurnstileSecret },
            { "response", token }
        };

        var postContent = new FormUrlEncodedContent(parameters);
        var siteVerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

        try
        {
            var response = await _httpClient.PostAsync(siteVerifyUrl, postContent);

            var turnstileResponse = await response.Content.ReadFromJsonAsync<TurnstileResponseDto>();
            return turnstileResponse?.Success ?? false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}