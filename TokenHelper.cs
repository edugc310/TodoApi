using System;
using System.IO;
using Newtonsoft.Json;

namespace TodoApi
{
    public static class TokenHelper
    {
        public static string GetToken(string userName, string password)
        {
            // do a db lookup to confirm the userName and password

            // create the token
            var token = new Token
            {
                UserId = 10,
                Expires = DateTime.UtcNow.AddMinutes(1),
            };

            var jsonString = JsonConvert.SerializeObject(token);
            // { UserId: 10, "Expires": etc }

            var encryptedJsonString = Crypto.EncryptStringAES(jsonString);
            // ADFEC$#$DFD#$#$#CD==

            return encryptedJsonString;
        }

        // ADFEC$#$DFD#$#$#CD==
        public static Token DecodeToken(string token)
        {
            var decryptedJsonString = Crypto.DecryptStringAES(token);
            // { UserId: 10, "Expires": etc }

            var tokenObject = JsonConvert.DeserializeObject<Token>(decryptedJsonString);

            if (tokenObject.Expires < DateTime.UtcNow)
            {
                return null;
            }

            return tokenObject;
        }
    }
}
