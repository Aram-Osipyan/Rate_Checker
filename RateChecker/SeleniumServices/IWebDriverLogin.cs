﻿using RateChecker.Domain;

namespace RateChecker.SeleniumServices
{
    public interface IWebDriverLogin
    {
        Task<(string token, string cookie)> Login(TokenRefreshInput input);
    }
}