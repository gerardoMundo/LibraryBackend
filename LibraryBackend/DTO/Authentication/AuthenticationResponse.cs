﻿namespace LibraryBackend.DTO.Authentication
{
    public class AuthenticationResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string ErrorMessage { get; set; }
    }
}
