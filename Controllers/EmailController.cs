using Microsoft.AspNetCore.Mvc;
using MimeKit;

namespace EmailAnalyzer.Controllers;

[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    private readonly ILogger<EmailController> _logger;

    public EmailController(ILogger<EmailController> logger)
    {
        _logger = logger;
    }

    [HttpPost("UploadMbox")]
    [Consumes("multipart/form-data")]
    public IActionResult UploadMbox(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded or the file is empty.");
        }

        try
        {
            using var stream = file.OpenReadStream();

            var message = MessageParser(stream, 1).FirstOrDefault();

            // Log details about the parsed message
            _logger.LogInformation("Email Subject: {Subject}", message.Subject);
            _logger.LogInformation("From: {From}", message.From);
            _logger.LogInformation("To: {To}", message.To);

            return Ok(new
            {
                Subject = message.Subject,
                From = message.From.Mailboxes.Select(m => m.Address).ToList(),
                To = message.To.Mailboxes.Select(m => m.Address).ToList(),
                HasAttachments = message.Attachments.Any()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse the uploaded file.");
            return BadRequest("Failed to parse the uploaded file as an email.");
        }
    }

    [HttpPost("UploadElm")]
    [Consumes("multipart/form-data")]
    public IActionResult UploadElm(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file was uploaded or the file is empty.");
        }

        try
        {
            using var stream = file.OpenReadStream();

            var message = MessageParser(stream, 2).FirstOrDefault();

            // Log details about the parsed message
            _logger.LogInformation("Email Subject: {Subject}", message.Subject);
            _logger.LogInformation("From: {From}", message.From);
            _logger.LogInformation("To: {To}", message.To);

            return Ok(new
            {
                Subject = message.Subject,
                From = message.From.Mailboxes.Select(m => m.Address).ToList(),
                To = message.To.Mailboxes.Select(m => m.Address).ToList(),
                HasAttachments = message.Attachments.Any()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse the uploaded file.");
            return BadRequest("Failed to parse the uploaded file as an email.");
        }
    }

    private List<MimeMessage> MessageParser(Stream fileStream, int type)
    {
        var mimeMessageList = new List<MimeMessage>();
        switch (type)
        {
            case 1:
                var parser = new MimeParser(fileStream, MimeFormat.Mbox);
                while (!parser.IsEndOfStream)
                {
                    var messagePart = parser.ParseMessage();
                    mimeMessageList.Add(messagePart);
                }
                return mimeMessageList;
            case 2:
                var reader = new StreamReader(fileStream);
                var fileContent = reader.ReadToEnd();
                reader.Dispose();
                var contentStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));
                var message = MimeMessage.Load(contentStream);
                contentStream.Dispose();
                return [message];
            default:
                throw new Exception("Unsuported type");
        }
    }

}
