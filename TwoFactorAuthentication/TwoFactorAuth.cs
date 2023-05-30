using Prysm.AppVision.Common;
using Prysm.AppVision.Data;
using Prysm.AppVision.SDK;
using Prysm.AppVision.Server;
using Prysm.AppVision.Server.Authentication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace TwoFactorAuthentication
{
    public class TwoFactor : AppUserLogin
    {
        public override string Name => nameof(TwoFactor);

        public override void Initialize(IServer server) { }

        public override UserRow Login(NameValue[] args)
        {
            Housekeeping();

            // 2factorauth not requested
            var auth = Helper.GetParameter("Authentication", args);
            if (auth != Name)
                return null;

            // 1st factor : check name and password against AppVision repository
            var user = base.Login(args);

            var code = Helper.GetParameter("Code", args);

            // No code is provided, generate a random code and send it to the user by mail.
            if (string.IsNullOrWhiteSpace(code))
            {
                Send2ndFactor(user);

                throw new WebFaultException<string>("Continuation needed", HttpStatusCode.Unauthorized);
            }
            // 2nd factor : check if the provided code is valid
            else if (candidates.TryRemove(user.Name, out var candidate))
            {
                if (candidate.Code == code)
                    return user;
            }
            throw new LoginException(LoginResult.IncorrectUserPassword, "Invalid code for " + user.Name);
        }


        void Send2ndFactor(UserRow user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
                throw new LoginException(LoginResult.InvalidUser, $"Email address is not set for user '{user.Name}'.");

            var cand = new Candidate
            {
                User = user,
                Date = DateTime.UtcNow,
                Code = random.Next(9999).ToString("0000")
            };

            var msg = $@"
Hi {user.FullName},

To sign in AppVision, please in the following code: 
	
	<h2>{cand.Code}</h2>

This code is valid until {cand.Date.AddMinutes(10).ToLocalTime()}.

Best regards,
The AppVision team
";

            // Send code by mail. Please ensure the server configuration contains valid SMTP settings.
            Helper.SendMail(user.Email, "AppVision sign in", msg);

            // Store issued code in memory
            candidates[user.Name] = cand;
        }


        static Random random = new Random();
        static ConcurrentDictionary<string, Candidate> candidates = new ConcurrentDictionary<string, Candidate>();

        struct Candidate
        {
            public UserRow User;
            public string Code;
            public DateTime Date;
        }

        void Housekeeping()
        {
            foreach (var expiredCodes in
                from _ in candidates.Values
                where _.Date.AddMinutes(3) > DateTime.UtcNow
                select _.User.Name)
                candidates.TryRemove(expiredCodes, out _);
        }
    }
}