using System;

namespace GuestFlow.Application.Operations.Setting.Dtos
{
    /// <summary>
    /// Ayar DTO
    /// </summary>
    public class SettingDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // System, Email, Currency, Pdf, etc.
        public string? Description { get; set; }
        public string? DataType { get; set; } // string, int, bool, decimal, etc.
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    /// <summary>
    /// Ayar güncelleme DTO
    /// </summary>
    public class UpdateSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Ayar kategorileri DTO
    /// </summary>
    public class SettingCategoryDto
    {
        public string Category { get; set; } = string.Empty;
        public int Count { get; set; }
        public List<SettingDto> Settings { get; set; } = new List<SettingDto>();
    }

    /// <summary>
    /// Sistem ayarları özeti DTO
    /// </summary>
    public class SystemSettingsSummaryDto
    {
        public bool MaintenanceMode { get; set; }
        public string DefaultCurrency { get; set; } = string.Empty;
        public string? EmailSmtpHost { get; set; }
        public bool EmailEnabled { get; set; }
        public string? PdfOutputPath { get; set; }
        public Dictionary<string, string> AllSettings { get; set; } = new Dictionary<string, string>();
    }
}

