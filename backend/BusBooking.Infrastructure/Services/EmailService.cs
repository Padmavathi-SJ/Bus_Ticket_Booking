using BusBooking.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace BusBooking.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendBookingConfirmationEmailAsync(
        string toEmail,
        string passengerName,
        string bookingReference,
        string busName,
        string busNumber,
        string source,
        string destination,
        string pickupPoint,
        string dropPoint,
        DateTime tripDate,
        string departureTime,
        string arrivalTime,
        List<string> seatNumbers,
        decimal totalAmount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            
            // From
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            
            // To
            message.To.Add(new MailboxAddress(passengerName, toEmail));
            
            // Subject
            message.Subject = $"Booking Confirmation - {bookingReference}";
            
            // Body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = GenerateBookingConfirmationHtml(
                    passengerName,
                    bookingReference,
                    busName,
                    busNumber,
                    source,
                    destination,
                    pickupPoint,
                    dropPoint,
                    tripDate,
                    departureTime,
                    arrivalTime,
                    seatNumbers,
                    totalAmount)
            };
            
            message.Body = bodyBuilder.ToMessageBody();
            
            // Send email
            using var client = new SmtpClient();
            
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPass = _configuration["EmailSettings:SmtpPass"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
            
            Console.WriteLine($"[EMAIL] Connecting to SMTP server: {smtpHost}:{smtpPort}");
            
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);
            
            Console.WriteLine($"[EMAIL] Authenticating with user: {smtpUser}");
            await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
            
            Console.WriteLine($"[EMAIL] Sending email to: {toEmail}");
            await client.SendAsync(message, cancellationToken);
            
            await client.DisconnectAsync(true, cancellationToken);
            
            Console.WriteLine($"[EMAIL] Booking confirmation email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL-ERROR] Failed to send email to {toEmail}: {ex.Message}");
            Console.WriteLine($"[EMAIL-ERROR] Stack trace: {ex.StackTrace}");
            // Don't throw - we don't want email failures to break the booking process
        }
    }

    private string GenerateBookingConfirmationHtml(
        string passengerName,
        string bookingReference,
        string busName,
        string busNumber,
        string source,
        string destination,
        string pickupPoint,
        string dropPoint,
        DateTime tripDate,
        string departureTime,
        string arrivalTime,
        List<string> seatNumbers,
        decimal totalAmount)
    {
        var seatsString = string.Join(", ", seatNumbers);
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .content {{
            background: #f8f9fa;
            padding: 30px;
            border-radius: 0 0 10px 10px;
        }}
        .booking-ref {{
            background: white;
            padding: 15px;
            border-left: 4px solid #667eea;
            margin: 20px 0;
            font-size: 18px;
            font-weight: bold;
        }}
        .section {{
            background: white;
            padding: 20px;
            margin: 20px 0;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .section h2 {{
            color: #667eea;
            margin-top: 0;
            font-size: 20px;
            border-bottom: 2px solid #667eea;
            padding-bottom: 10px;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #eee;
        }}
        .info-row:last-child {{
            border-bottom: none;
        }}
        .label {{
            font-weight: bold;
            color: #666;
        }}
        .value {{
            color: #333;
        }}
        .route {{
            background: #e3f2fd;
            padding: 15px;
            border-radius: 8px;
            margin: 15px 0;
        }}
        .route-arrow {{
            text-align: center;
            color: #667eea;
            font-size: 24px;
            margin: 10px 0;
        }}
        .seats {{
            background: #fff3e0;
            padding: 15px;
            border-radius: 8px;
            text-align: center;
            font-size: 18px;
            font-weight: bold;
            color: #f57c00;
        }}
        .total {{
            background: #c8e6c9;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
            color: #2e7d32;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            color: #666;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 2px solid #eee;
        }}
        .important {{
            background: #fff3cd;
            border-left: 4px solid #ffc107;
            padding: 15px;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎉 Booking Confirmed!</h1>
        <p>Your bus ticket has been successfully booked</p>
    </div>
    
    <div class='content'>
        <p>Dear <strong>{passengerName}</strong>,</p>
        <p>Thank you for booking with Bus Booking System. Your booking has been confirmed.</p>
        
        <div class='booking-ref'>
            📋 Booking Reference: {bookingReference}
        </div>
        
        <div class='section'>
            <h2>🚌 Bus Details</h2>
            <div class='info-row'>
                <span class='label'>Bus Name:</span>
                <span class='value'>{busName}</span>
            </div>
            <div class='info-row'>
                <span class='label'>Bus Number:</span>
                <span class='value'>{busNumber}</span>
            </div>
        </div>
        
        <div class='section'>
            <h2>📍 Journey Details</h2>
            <div class='route'>
                <div class='info-row'>
                    <span class='label'>From:</span>
                    <span class='value'>{source}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Pickup Point:</span>
                    <span class='value'>{pickupPoint}</span>
                </div>
                <div class='route-arrow'>↓</div>
                <div class='info-row'>
                    <span class='label'>To:</span>
                    <span class='value'>{destination}</span>
                </div>
                <div class='info-row'>
                    <span class='label'>Drop Point:</span>
                    <span class='value'>{dropPoint}</span>
                </div>
            </div>
            
            <div class='info-row'>
                <span class='label'>📅 Journey Date:</span>
                <span class='value'>{tripDate:dddd, MMMM dd, yyyy}</span>
            </div>
            <div class='info-row'>
                <span class='label'>🕐 Departure Time:</span>
                <span class='value'>{departureTime}</span>
            </div>
            <div class='info-row'>
                <span class='label'>🕐 Arrival Time:</span>
                <span class='value'>{arrivalTime}</span>
            </div>
        </div>
        
        <div class='section'>
            <h2>💺 Seat Information</h2>
            <div class='seats'>
                Seat Number(s): {seatsString}
            </div>
        </div>
        
        <div class='total'>
            💰 Total Amount Paid: ₹{totalAmount:N2}
        </div>
        
        <div class='important'>
            <strong>⚠️ Important Instructions:</strong>
            <ul>
                <li>Please arrive at the pickup point at least 15 minutes before departure</li>
                <li>Carry a valid ID proof for verification</li>
                <li>Keep this booking reference handy</li>
                <li>Contact support for any changes or cancellations</li>
            </ul>
        </div>
        
        <div class='footer'>
            <p>For any queries, please contact us at:<br>
            📧 <a href='mailto:parkingsystemcloud@gmail.com'>parkingsystemcloud@gmail.com</a></p>
            <p style='color: #999; font-size: 12px; margin-top: 20px;'>
                This is an automated email. Please do not reply to this message.
            </p>
        </div>
    </div>
</body>
</html>";
    }

    public async Task SendBookingCancellationEmailAsync(
        string toEmail,
        string passengerName,
        string bookingReference,
        string busName,
        string busNumber,
        string source,
        string destination,
        DateTime tripDate,
        List<string> seatNumbers,
        decimal refundAmount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MimeMessage();
            
            // From
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            
            // To
            message.To.Add(new MailboxAddress(passengerName, toEmail));
            
            // Subject
            message.Subject = $"Booking Cancellation - {bookingReference}";
            
            // Body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = GenerateBookingCancellationHtml(
                    passengerName,
                    bookingReference,
                    busName,
                    busNumber,
                    source,
                    destination,
                    tripDate,
                    seatNumbers,
                    refundAmount)
            };
            
            message.Body = bodyBuilder.ToMessageBody();
            
            // Send email
            using var client = new SmtpClient();
            
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var smtpUser = _configuration["EmailSettings:SmtpUser"];
            var smtpPass = _configuration["EmailSettings:SmtpPass"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
            
            Console.WriteLine($"[EMAIL-CANCEL] Connecting to SMTP server: {smtpHost}:{smtpPort}");
            
            await client.ConnectAsync(smtpHost, smtpPort, enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);
            
            Console.WriteLine($"[EMAIL-CANCEL] Authenticating with user: {smtpUser}");
            await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
            
            Console.WriteLine($"[EMAIL-CANCEL] Sending cancellation email to: {toEmail}");
            await client.SendAsync(message, cancellationToken);
            
            await client.DisconnectAsync(true, cancellationToken);
            
            Console.WriteLine($"[EMAIL-CANCEL] Cancellation email sent successfully to {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL-CANCEL-ERROR] Failed to send cancellation email to {toEmail}: {ex.Message}");
            Console.WriteLine($"[EMAIL-CANCEL-ERROR] Stack trace: {ex.StackTrace}");
            // Don't throw - we don't want email failures to break the cancellation process
        }
    }

    private string GenerateBookingCancellationHtml(
        string passengerName,
        string bookingReference,
        string busName,
        string busNumber,
        string source,
        string destination,
        DateTime tripDate,
        List<string> seatNumbers,
        decimal refundAmount)
    {
        var seatsString = string.Join(", ", seatNumbers);
        
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
        }}
        .header {{
            background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
            color: white;
            padding: 30px;
            text-align: center;
            border-radius: 10px 10px 0 0;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .content {{
            background: #f8f9fa;
            padding: 30px;
            border-radius: 0 0 10px 10px;
        }}
        .booking-ref {{
            background: white;
            padding: 15px;
            border-left: 4px solid #ef4444;
            margin: 20px 0;
            font-size: 18px;
            font-weight: bold;
        }}
        .section {{
            background: white;
            padding: 20px;
            margin: 20px 0;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .section h2 {{
            color: #ef4444;
            margin-top: 0;
            font-size: 20px;
            border-bottom: 2px solid #ef4444;
            padding-bottom: 10px;
        }}
        .info-row {{
            display: flex;
            justify-content: space-between;
            padding: 10px 0;
            border-bottom: 1px solid #eee;
        }}
        .info-row:last-child {{
            border-bottom: none;
        }}
        .label {{
            font-weight: bold;
            color: #666;
        }}
        .value {{
            color: #333;
        }}
        .seats {{
            background: #fee2e2;
            padding: 15px;
            border-radius: 8px;
            text-align: center;
            font-size: 18px;
            font-weight: bold;
            color: #991b1b;
        }}
        .refund {{
            background: #dcfce7;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
            color: #166534;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            color: #666;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 2px solid #eee;
        }}
        .important {{
            background: #fef3c7;
            border-left: 4px solid #f59e0b;
            padding: 15px;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>❌ Booking Cancelled</h1>
        <p>Your booking has been successfully cancelled</p>
    </div>
    
    <div class='content'>
        <p>Dear <strong>{passengerName}</strong>,</p>
        <p>We confirm that your booking has been cancelled as per your request.</p>
        
        <div class='booking-ref'>
            📋 Booking Reference: {bookingReference}
        </div>
        
        <div class='section'>
            <h2>🚌 Bus Details</h2>
            <div class='info-row'>
                <span class='label'>Bus Name:</span>
                <span class='value'>{busName}</span>
            </div>
            <div class='info-row'>
                <span class='label'>Bus Number:</span>
                <span class='value'>{busNumber}</span>
            </div>
        </div>
        
        <div class='section'>
            <h2>📍 Journey Details</h2>
            <div class='info-row'>
                <span class='label'>Route:</span>
                <span class='value'>{source} → {destination}</span>
            </div>
            <div class='info-row'>
                <span class='label'>📅 Journey Date:</span>
                <span class='value'>{tripDate:dddd, MMMM dd, yyyy}</span>
            </div>
        </div>
        
        <div class='section'>
            <h2>💺 Cancelled Seats</h2>
            <div class='seats'>
                Seat Number(s): {seatsString}
            </div>
        </div>
        
        <div class='refund'>
            💰 Refund Amount: ₹{refundAmount:N2}
        </div>
        
        <div class='important'>
            <strong>ℹ️ Refund Information:</strong>
            <ul>
                <li>Your refund will be processed within 5-7 business days</li>
                <li>The amount will be credited to your original payment method</li>
                <li>You will receive a separate notification once the refund is processed</li>
                <li>For any queries, please contact our support team</li>
            </ul>
        </div>
        
        <div class='footer'>
            <p>For any queries, please contact us at:<br>
            📧 <a href='mailto:parkingsystemcloud@gmail.com'>parkingsystemcloud@gmail.com</a></p>
            <p style='color: #999; font-size: 12px; margin-top: 20px;'>
                This is an automated email. Please do not reply to this message.
            </p>
        </div>
    </div>
</body>
</html>";
    }
}

