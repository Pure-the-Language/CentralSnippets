/* Send Plain Text Email
Version: v0.1
*/

using System.Net;
using System.Net.Mail;

#region With Login
void Send(string from, string to, string title, string content, string server, int port, string username, string password)
{
    SmtpClient smtpClient = new(server)
    {
        Port = port,
        Credentials = new NetworkCredential(username, password),
        EnableSsl = true,
    };
        
    smtpClient.Send(from, to, title, content);
}
void SendHTML(string from, string to, string title, string html, string server, int port, string username, string password)
{
    SmtpClient smtpClient = new(server)
    {
        Port = port,
        Credentials = new NetworkCredential(username, password),
        DeliveryMethod = SmtpDeliveryMethod.Network,
        UseDefaultCredentials = false,
        EnableSsl = true,
    };
    MailMessage mailMessage = new()
    {
        From = new MailAddress(from),
        Subject = title,
        Body = html,
        IsBodyHtml = true,
    };
    mailMessage.To.Add(to);
    smtpClient.Send(mailMessage);
}
#endregion

#region No Login
void Send(string from, string to, string title, string content, string server, int port = 25)
{
    SmtpClient smtpClient = new(server);
    smtpClient.Send(from, to, title, content);
}

void SendHTML(string from, string to, string title, string html, string server, int port = 25)
{
    SmtpClient smtpClient = new(server);
    MailMessage mailMessage = new()
    {
        From = new MailAddress(from),
        Subject = title,
        Body = html,
        IsBodyHtml = true,
    };
    mailMessage.To.Add(to);
    smtpClient.Send(mailMessage);
}
#endregion

// Doc
WriteLine("""
Methods - With Login:
  void Send(string from, string to, string title, string content, string server, int port, string username, string password): Send with username and password
  void SendHTML(string from, string to, string title, string html, string server, int port, string username, string password): Send with username and password

Methods - Without Login:
  void Send(string from, string to, string title, string content, string server, int port = 25): Use default login and port
  void SendHTML(string from, string to, string title, string html, string server, int port = 25): Use default login and port
""");