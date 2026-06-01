using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SharedKernel.Interfaces;

namespace Api.Extensions.Filters;

public class WebhookSecretFilter : IActionFilter, IScoped
{
    private readonly string _secret;

    public WebhookSecretFilter(IConfiguration configuration)
    {
        _secret = configuration["Webhook:Secret"] ?? throw new InvalidOperationException("Webhook:Secret não configurado.");
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var request = context.HttpContext.Request;

        var signatureHeader = request.Headers["X-Webhook-Signature"].ToString();
        if (string.IsNullOrEmpty(signatureHeader))
        {
            context.Result = new UnauthorizedObjectResult("Header X-Webhook-Signature ausente.");
            return;
        }

        request.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var rawBody = reader.ReadToEndAsync().GetAwaiter().GetResult();
        request.Body.Seek(0, SeekOrigin.Begin);

        var keyBytes = Encoding.UTF8.GetBytes(_secret);
        var payloadBytes = Encoding.UTF8.GetBytes(rawBody);

        using var hmac = new HMACSHA256(keyBytes);
        var computedHash = hmac.ComputeHash(payloadBytes);
        var computedSignature = $"sha256={Convert.ToHexString(computedHash).ToLowerInvariant()}";

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedSignature),
                Encoding.UTF8.GetBytes(signatureHeader)))
        {
            context.Result = new UnauthorizedObjectResult("Assinatura inválida.");
            return;
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}