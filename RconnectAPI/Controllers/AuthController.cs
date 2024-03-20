using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RconnectAPI.Models;
using RconnectAPI.Services;

namespace RconnectAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private const string CompanyName = "R-Konnect";
        private static string EmailSender;
        private static string Password;


        public AuthController(UserService userService)
        {
            EmailSender = Environment.GetEnvironmentVariable("RESET_PASSWORD_MAIL");
            Password = Environment.GetEnvironmentVariable("RESET_PASSWORD_PASSWORD");

            _userService = userService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Connexion([FromBody] UserLoginData loginData)
        {
            var user = await _userService.Login(loginData.Email, loginData.Password);
            if (user == null)
            {
                return Unauthorized("Identifiants incorrects.");
            }

            var token = _userService.GenerateJwt(user);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Inscription([FromBody] UserRegisterData newUser)
        {
            var user = await _userService.RegisterAsync(newUser.Username, newUser.Email, newUser.Password, newUser.Birthdate, newUser.Firstname, newUser.Lastname);
            if (user == null)
            {
                return BadRequest();
            }
            return Ok(user);
        }

        [HttpGet("forgot_password/{email}")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null)
            {
                return NotFound("Utilisateur non trouv�.");
            }

            var token = GenerateResetToken();
            user.ResetToken = token;
            user.TokenTime = DateTime.Now.AddHours(1);
            await _userService.UpdateAsync(user.Id, user);
            var resetLink = $"https://rconnect-api.azurewebsites.net/Auth/reset_password/{token}";
            SendResetPasswordEmail(email, resetLink);

            return Ok("Un e-mail de r�initialisation a �t� envoy� � votre adresse e-mail.");
        }

        private string GenerateResetToken()
        {
            return Guid.NewGuid().ToString();
        }

        private void SendResetPasswordEmail(string email, string resetLink)
        {
            using (var message = new MailMessage())
            {
                message.From = new MailAddress(EmailSender, CompanyName);
                message.To.Add(email);
                message.Subject = "R�initialisation de mot de passe";
                message.IsBodyHtml = true; 

                // logo 
                Attachment logoAttachment = new Attachment("Assets/images/logo.png"); 
                logoAttachment.ContentId = "logo"; 
                message.Attachments.Add(logoAttachment);

                // Contenu du message au format HTML avec style
                message.Body = $@"
            <html>
            <head>
                <style>
                    /* Ajoutez ici vos styles CSS */
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f4;
                        padding: 20px;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        background-color: #fff;
                        padding: 40px;
                        border-radius: 8px;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    .button {{
                        display: inline-block;
                        padding: 10px 20px;
                        background-color: #007bff;
                        color: #fff;
                        text-decoration: none;
                        border-radius: 5px;
                    }}
                    .logo {{
                        max-width: 80px; 
                        margin-bottom: 20px;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <h2>R�initialisation de mot de passe</h2>
                    <img class='logo' src='cid:logo' alt='Logo'>
                    <p>Cliquez sur le lien suivant pour r�initialiser votre mot de passe :</p>
                    <p><a class='button' href='{resetLink}'>R�initialiser le mot de passe</a></p>
                    <p>Si le bouton ci-dessus ne fonctionne pas, vous pouvez �galement copier et coller le lien suivant dans votre navigateur :</p>
                    <p>{resetLink}</p>
                    <p>Merci,<br>L'�quipe R-Konnect</p>

                </div>
            </body>
            </html>";
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential(EmailSender, Password);
                    smtpClient.Send(message);
                }
            }
        }

     //   [HttpGet("check_token/{token}")]
      //  public async Task<IActionResult> CheckToken(string token)
     //   {
     //       var user = await _userService.GetByResetTokenAsync(token);

      //      if (user == null || user.TokenTime == null || user.ResetToken == null)
       //     {
       //         return NotFound("Lien invalide ou expir�.");
      //      }

       //     if (DateTime.UtcNow > user.TokenTime.Value)
     //       {
      //          return BadRequest("Lien expir�.");
      //      }


            // Le token est valide, retourner une r�ponse avec l'ID de l'utilisateur
   //         return Ok(new { UserId = user.Id });
   //     }



        [HttpPost("reset_password/{token}")]
        public async Task<IActionResult> ResetPassword(string token, [FromBody] ResetPasswordModel model)
        {
            var user = await _userService.GetByResetTokenAsync(token);

            if (user == null || user.TokenTime == null || user.ResetToken == null)
            {
                return NotFound("Lien invalide ou expir�.");
            }

            if (DateTime.UtcNow > user.TokenTime.Value)
            {
                return BadRequest("Lien expir�.");
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _userService.UpdateAsync(user.Id, user);

            return Ok("Mot de passe r�initialis� avec succ�s.");
        }
    }
}
