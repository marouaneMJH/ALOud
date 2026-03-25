using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ALOud.Services.Security
{
    /// <summary>
    /// Service for generating and validating JWT tokens
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="email">User email</param>
        /// <param name="isAdmin">Whether user has admin privileges</param>
        /// <returns>JWT token string</returns>
        string GenerateToken(string userId, string email, bool isAdmin = false);

        /// <summary>
        /// Validates a JWT token and extracts claims
        /// </summary>
        /// <param name="token">JWT token to validate</param>
        /// <returns>Claims principal if valid, null otherwise</returns>
        ClaimsPrincipal? ValidateToken(string token);
    }

    /// <summary>
    /// Implementation of JWT token service
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly ILogger<JwtTokenService> _logger;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public JwtTokenService(
            IConfiguration configuration,
            ILogger<JwtTokenService> logger)
        {
            _logger = logger;

            // Get configuration from appsettings or environment variables
            _secretKey = configuration["Jwt:SecretKey"] 
                ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? "ALOudSecretKeyForJwtTokenGenerationPleaseChangeInProduction123456789!";

            _issuer = configuration["Jwt:Issuer"]
                ?? Environment.GetEnvironmentVariable("JWT_ISSUER")
                ?? "ALOud";

            _audience = configuration["Jwt:Audience"]
                ?? Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                ?? "ALOudAPI";

            if (!int.TryParse(
                configuration["Jwt:ExpirationMinutes"]
                ?? Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES"),
                out _expirationMinutes))
            {
                _expirationMinutes = 60; // Default 1 hour
            }

            _logger.LogInformation($"JWT configured: Issuer={_issuer}, Audience={_audience}, Expiration={_expirationMinutes}min");
        }

        /// <summary>
        /// Generates a JWT token for the specified user
        /// </summary>
        public string GenerateToken(string userId, string email, bool isAdmin = false)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Email, email),
                    new Claim("aud", _audience),
                };

                if (isAdmin)
                {
                    claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                }

                var token = new JwtSecurityToken(
                    issuer: _issuer,
                    audience: _audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
                    signingCredentials: credentials
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token");
                throw;
            }
        }

        /// <summary>
        /// Validates a JWT token and extracts claims
        /// </summary>
        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid JWT token");
                return null;
            }
        }
    }
}
