namespace SalonCare.Core.Enums;

/// <summary>
/// Phân khúc khách hàng theo mô hình RFM (Recency - Frequency - Monetary).
/// </summary>
public enum RfmSegment
{
    /// <summary>
    /// Khách hàng VIP hàng đầu: Mới ghé gần đây, tần suất cao, chi tiêu lớn.
    /// </summary>
    Champions = 1,

    /// <summary>
    /// Khách hàng thân thiết: Thường xuyên ghé salon và phản hồi tích cực.
    /// </summary>
    Loyal = 2,

    /// <summary>
    /// Khách hàng tiềm năng: Khách mới hoặc khách có xu hướng chi tiêu tăng.
    /// </summary>
    PotentialLoyalist = 3,

    /// <summary>
    /// Khách hàng có nguy cơ rời bỏ: Đã ngưng hoạt động trên 60 ngày.
    /// </summary>
    AtRisk = 4,

    /// <summary>
    /// Khách hàng đã mất: Đã ngưng hoạt động trên 90-120 ngày, tỷ lệ No-show cao trong quá khứ.
    /// </summary>
    Lost = 5
}
