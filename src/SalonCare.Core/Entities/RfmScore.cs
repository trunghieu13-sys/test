using SalonCare.Core.Enums;

namespace SalonCare.Core.Entities;

/// <summary>
/// Thực thể Lưu trữ điểm số phân khúc khách hàng theo mô hình RFM.
/// </summary>
public class RfmScore
{
    public int RfmScoreId { get; set; }

    /// <summary>
    /// ID Khách hàng được chấm điểm.
    /// </summary>
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    /// <summary>
    /// Recency: Số ngày kể từ lần ghé thăm gần nhất của khách hàng.
    /// </summary>
    public int RecencyDays { get; set; }

    /// <summary>
    /// Frequency: Tổng số lần hoàn thành dịch vụ tại salon.
    /// </summary>
    public int Frequency { get; set; }

    /// <summary>
    /// Monetary: Tổng số tiền chi tiêu tích lũy tại salon (VNĐ).
    /// </summary>
    public decimal Monetary { get; set; }

    /// <summary>
    /// Phân khúc khách hàng xác định (Champions, Loyal, AtRisk, Lost,...).
    /// </summary>
    public RfmSegment RfmSegment { get; set; }

    /// <summary>
    /// Thời điểm hệ thống tính toán điểm số RFM này.
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
