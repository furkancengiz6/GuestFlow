using GuestFlow.Application.Models.Responses.Auth;
using GuestFlow.Application.Operations.QRCode;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Auth
{
    /// <summary>
    /// Two-Factor Authentication (2FA) service implementation
    /// Uses TOTP (Time-based One-Time Password) - Google Authenticator compatible
    /// </summary>
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<PersonnelEntity> _personnelRepository;
        private readonly IQRCodeService _qrCodeService;
        private readonly IDataProtector _dataProtection;
        private readonly ILogger<TwoFactorService> _logger;

        public TwoFactorService(
            IUnitOfWork unitOfWork,
            IRepository<PersonnelEntity> personnelRepository,
            IQRCodeService qrCodeService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<TwoFactorService> logger)
        {
            _unitOfWork = unitOfWork;
            _personnelRepository = personnelRepository;
            _qrCodeService = qrCodeService;
            _dataProtection = dataProtectionProvider.CreateProtector("TwoFactor");
            _logger = logger;
        }

        public bool IsRequiredForUserType(UserType userType)
        {
            // 2FA is required for Admin and Owner only
            return userType == UserType.Admin || userType == UserType.Owner;
        }

        public async Task<bool> IsEnabledAsync(int personnelId)
        {
            var personnel = await _personnelRepository.GetByIdAsync(personnelId);
            return personnel?.TwoFactorEnabled ?? false;
        }

        public async Task<TwoFactorSetupResponse> GenerateSetupAsync(int personnelId, string email, string issuer = "GuestFlow")
        {
            try
            {
                var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                if (personnel == null)
                    throw new Exception("User not found");

                // Generate a new secret key (Base32 encoded)
                var secretBytes = KeyGeneration.GenerateRandomKey(20); // 160 bits
                var secret = Base32Encoding.ToString(secretBytes);

                // Generate recovery codes (10 codes, 8 characters each)
                var recoveryCodes = GenerateRecoveryCodes(10);
                var encryptedRecoveryCodes = _dataProtection.Protect(JsonSerializer.Serialize(recoveryCodes));

                // Store secret and recovery codes (encrypted) temporarily
                // They will be saved when user verifies the code
                personnel.TwoFactorSecret = _dataProtection.Protect(secret);
                personnel.TwoFactorRecoveryCodes = encryptedRecoveryCodes;
                await _personnelRepository.UpdateAsync(personnel);
                await _unitOfWork.SaveChangesAsync();

                // Generate QR code data URI (otpauth:// URL format)
                var qrCodeData = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(email)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}&algorithm=SHA1&digits=6&period=30";
                
                var qrCodeResult = await _qrCodeService.GenerateQRCodeAsync(qrCodeData, 300);
                var qrCodeDataUri = qrCodeResult.IsSuccess 
                    ? $"data:image/png;base64,{qrCodeResult.Data.Base64Image}" 
                    : string.Empty;

                // Format secret for manual entry (add spaces every 4 characters)
                var manualEntryKey = string.Join(" ", Enumerable.Range(0, secret.Length / 4)
                    .Select(i => secret.Substring(i * 4, 4)));

                return new TwoFactorSetupResponse
                {
                    Secret = secret, // Return unencrypted for QR code generation
                    QrCodeDataUri = qrCodeDataUri,
                    ManualEntryKey = manualEntryKey,
                    RecoveryCodes = recoveryCodes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"2FA setup generation failed for personnel {personnelId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> VerifyAndEnableAsync(int personnelId, string code)
        {
            try
            {
                var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                if (personnel == null || string.IsNullOrEmpty(personnel.TwoFactorSecret))
                    return false;

                // Decrypt secret
                var secret = _dataProtection.Unprotect(personnel.TwoFactorSecret);
                var secretBytes = Base32Encoding.ToBytes(secret);

                // Verify TOTP code
                var totp = new Totp(secretBytes);
                var isValid = totp.VerifyTotp(code, out var timeStepMatched, new VerificationWindow(2, 2)); // Allow 2 time steps before/after

                if (!isValid)
                {
                    // Clear temporary secret if verification fails
                    personnel.TwoFactorSecret = null;
                    personnel.TwoFactorRecoveryCodes = null;
                    await _personnelRepository.UpdateAsync(personnel);
                    await _unitOfWork.SaveChangesAsync();
                    return false;
                }

                // Enable 2FA
                personnel.TwoFactorEnabled = true;
                personnel.TwoFactorSetupDate = DateTime.UtcNow;
                await _personnelRepository.UpdateAsync(personnel);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"2FA enabled for personnel {personnelId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"2FA verification failed for personnel {personnelId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyCodeAsync(int personnelId, string code)
        {
            try
            {
                var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                if (personnel == null || !personnel.TwoFactorEnabled || string.IsNullOrEmpty(personnel.TwoFactorSecret))
                    return false;

                // Decrypt secret
                var secret = _dataProtection.Unprotect(personnel.TwoFactorSecret);
                var secretBytes = Base32Encoding.ToBytes(secret);

                // Verify TOTP code
                var totp = new Totp(secretBytes);
                var isValid = totp.VerifyTotp(code, out var timeStepMatched, new VerificationWindow(2, 2));

                if (isValid)
                {
                    _logger.LogInformation($"2FA code verified for personnel {personnelId}");
                }
                else
                {
                    _logger.LogWarning($"2FA code verification failed for personnel {personnelId}");
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"2FA code verification error for personnel {personnelId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyRecoveryCodeAsync(int personnelId, string recoveryCode)
        {
            try
            {
                var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                if (personnel == null || !personnel.TwoFactorEnabled || string.IsNullOrEmpty(personnel.TwoFactorRecoveryCodes))
                    return false;

                // Decrypt recovery codes
                var encryptedCodes = personnel.TwoFactorRecoveryCodes;
                var codesJson = _dataProtection.Unprotect(encryptedCodes);
                var codes = JsonSerializer.Deserialize<List<string>>(codesJson) ?? new List<string>();

                // Check if recovery code exists
                var codeIndex = codes.IndexOf(recoveryCode);
                if (codeIndex == -1)
                {
                    _logger.LogWarning($"Invalid recovery code used for personnel {personnelId}");
                    return false;
                }

                // Remove used recovery code
                codes.RemoveAt(codeIndex);
                var updatedCodesJson = JsonSerializer.Serialize(codes);
                personnel.TwoFactorRecoveryCodes = codes.Count > 0 
                    ? _dataProtection.Protect(updatedCodesJson) 
                    : null;

                await _personnelRepository.UpdateAsync(personnel);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Recovery code used for personnel {personnelId}, {codes.Count} codes remaining");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Recovery code verification error for personnel {personnelId}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DisableAsync(int personnelId)
        {
            try
            {
                var personnel = await _personnelRepository.GetByIdAsync(personnelId);
                if (personnel == null)
                    return false;

                // Check if 2FA is required for this user type
                if (IsRequiredForUserType(personnel.UserType))
                {
                    _logger.LogWarning($"Cannot disable 2FA for {personnel.UserType} user {personnelId}");
                    return false;
                }

                personnel.TwoFactorEnabled = false;
                personnel.TwoFactorSecret = null;
                personnel.TwoFactorRecoveryCodes = null;
                personnel.TwoFactorSetupDate = null;

                await _personnelRepository.UpdateAsync(personnel);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"2FA disabled for personnel {personnelId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"2FA disable error for personnel {personnelId}: {ex.Message}");
                return false;
            }
        }

        private List<string> GenerateRecoveryCodes(int count)
        {
            var codes = new List<string>();
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            for (int i = 0; i < count; i++)
            {
                var code = new string(Enumerable.Range(0, 8)
                    .Select(_ => chars[random.Next(chars.Length)])
                    .ToArray());
                codes.Add(code);
            }

            return codes;
        }
    }
}
