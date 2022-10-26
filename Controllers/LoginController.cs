using Microsoft.AspNetCore.Mvc;
using Utils.Models;
using Utils.IRepository;
using System.Globalization;
using System.Web;

namespace Utils.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILoginRepository _loginRepo;
        private readonly IEmailRepository _emailRepo;

        public LoginController(ILoginRepository loginRepo, IEmailRepository emailRepo)
        {
            _loginRepo = loginRepo;
            _emailRepo = emailRepo;
        }
        /// <summary>
        /// This api will validate the user and if validated it will send otp to user's email address.
        /// It will send back the user_id, requestor_id (if user is a customer), and user role. 
        /// User role can be Admin, Super Admin, Investigator and Requestor (this is for customer)
        /// </summary>
        /// <param name="useremail"></param>
        /// <param name="password"></param>
        /// <returns>{user_id,requestor_id,role}</returns>
        /// <response code="200">Successfully validated</response>
        /// <response code="201">If user is not validated</response>
        [HttpGet]
        [Route("validatelogin/{useremail}/{password}")]
        public async Task<IActionResult> ValidateLogin(string useremail, string password)
        {
            Dictionary<string, string> loginData = await _loginRepo.ValidateLogin(useremail, password);
            if (loginData != null && loginData["user_id"] != "0")
            {
                Random random = new();
                string? otp = Convert.ToString(random.Next(11111, 99999));
                if (_emailRepo.EmailOTP(otp, useremail))
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    await _loginRepo.Add(entity: new OTPModel() { OTP = otp, Email = useremail });
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                return Ok(loginData);
            }
            return StatusCode(201, loginData);
        }
        /// <summary>
        /// It will validate OTP
        /// </summary>
        /// <param name="useremail"></param>
        /// <param name="otp"></param>
        /// <returns></returns>
        /// <response code="200">If successfully validated</response>
        /// <response code="201">If OTP expired</response>
        /// <response code="202">If OTP did not match</response>
        [HttpGet]
        [Route("validateotp/{useremail}/{otp}")]
        public async Task<ActionResult> ValidateOTP(string useremail, string otp)
        {

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            OTPModel otpData = await _loginRepo.GetByVar(useremail);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            if (otpData.OTP == otp)
                if (otpData.CreatedDate <= DateTime.UtcNow.AddMinutes(10))
                {
                    await _loginRepo.ExpireOTP(useremail, (DateTime)otpData.CreatedDate);
                    return Ok("Validated");
                }
                else
                    return StatusCode(201, "OTP Expired");
            else
                return StatusCode(202, "OTP did not match");
        }

        /// <summary>
        /// It will send reset password link to the user's email address
        /// </summary>
        /// <param name="useremail"></param>
        /// <returns></returns>
        /// <response code="200">If link sent to user's email.</response>
        /// <response code="201">If user does not exist.</response>
        /// <response code="500">If some error occured while validating user email.</response>
        [HttpGet]
        [Route("forgotpassword/{useremail}")]
        public async Task<ActionResult> ForgotPassword(string useremail)
        {
            string userName = await _loginRepo.IsUserExist(useremail);

            if (!string.IsNullOrWhiteSpace(userName))
            {
                string encryptedMsg = SimpleAes6.Encrypt(useremail+"^"+DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"));
                string encodedMsg = HttpUtility.UrlEncode(encryptedMsg);
                bool emailSent = _emailRepo.EmailForgotPasswordLink(useremail, userName, encodedMsg);

                if (emailSent)
                    return Ok($"Reset Password link sent to {userName}'s email");

            }
            else
            {
                return StatusCode(201, "User does not exist");
            }

            return StatusCode(500, userName);

        }
        /// <summary>
        /// Token will be decrypted. if it is not expired password will get reset.
        /// </summary>
        /// <param name="newpassword">MD5 Encrypted Password</param>
        /// <param name="token">Token is sent with reset password link on email</param>
        /// <returns></returns>
        /// <response code="200">Password updated</response>
        /// <response code="201">Token expired/Invalid token</response>
        /// <response code="500">Some error occured while updating password.</response>
        [HttpGet]
        [Route("resetpassword")]
        public async Task<ActionResult> ResetPassword(string newpassword, string token)
        {
            string decodedToken = HttpUtility.UrlDecode(token);
            string decryptedToken = SimpleAes6.DecryptToString(decodedToken);

            if (!string.IsNullOrEmpty(decryptedToken))
            {
                string[] tokenPart = decryptedToken.Split("^");
                if (Convert.ToDateTime(tokenPart[1], new CultureInfo("en-US")) <= DateTime.UtcNow.AddHours(24))
                {
                    var result = await _loginRepo.UpdatePassword(tokenPart[0], newpassword);

                    if (result)
                        return Ok("Password Updated");
                    else
                        return StatusCode(500, "Unable to Update Password");

                }
                else
                    return StatusCode(201,"Token Expired");
            }

            return StatusCode(201, "Invalid Token");
        }
    }
}
