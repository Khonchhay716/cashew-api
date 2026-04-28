using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;  // ← explicit alias fixes ambiguity

namespace CashewNuts.Application.Services;

public class EmailService
{
  private readonly IConfiguration _config;
  public EmailService(IConfiguration config) => _config = config;

  public async Task SendResetPasswordEmailAsync(string toEmail, string toName, string token)
  {
    var s = _config.GetSection("EmailSettings");
    var host = s["Host"]!;
    var port = int.Parse(s["Port"]!);
    var from = s["Email"]!;
    var pass = s["Password"]!;
    var name = s["DisplayName"]!;

    // var resetLink = $"http://localhost:5173/reset-password?token={token}";
    var frontendUrl = _config["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
    var resetLink = $"{frontendUrl}/reset-password?token={token}";

    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(name, from));
    message.To.Add(new MailboxAddress(toName, toEmail));
    message.Subject = "Reset Password — Cashew Nuts System";

    message.Body = new TextPart("html")
    {
      Text = $"""
            <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:32px;background:#f9fafb;border-radius:16px;">
              <div style="text-align:center;margin-bottom:24px;">
                <span style="font-size:48px;">🥜</span>
                <h1 style="color:#15803d;margin:8px 0;">Cashew Nuts System</h1>
              </div>
              <div style="background:white;border-radius:12px;padding:24px;">
                <p style="color:#374151;">សួស្ដី <strong>{toName}</strong>,</p>
                <p style="color:#374151;">អ្នកបានស្នើសុំ Reset Password។ ចុចប៊ូតុងខាងក្រោម:</p>
                <div style="text-align:center;margin:28px 0;">
                  <a href="{resetLink}"
                     style="background:#16a34a;color:white;padding:14px 32px;border-radius:12px;text-decoration:none;font-weight:bold;font-size:15px;">
                    Reset Password
                  </a>
                </div>
                <p style="color:#9ca3af;font-size:13px;">Link នេះនឹងផុតកំណត់ក្នុង <strong>30 នាទី</strong>។</p>
                <p style="color:#9ca3af;font-size:13px;">ប្រសិនបើអ្នកមិនបានស្នើ សូមព្រងើយកន្តើយ Email នេះ។</p>
              </div>
            </div>
            """
    };

    using var smtp = new SmtpClient();   // ← now unambiguous
    await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
    await smtp.AuthenticateAsync(from, pass);
    await smtp.SendAsync(message);
    await smtp.DisconnectAsync(true);
  }
}