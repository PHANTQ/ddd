﻿namespace AuthService.Model
{
    public class VerifiedEmail
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime VerifiedAt { get; set; }
    }

}
