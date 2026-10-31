using SalonCare.Core.Entities;
using SalonCare.Core.Enums;

namespace SalonCare.Core.Interfaces;

/// <summary>
/// Giao diện Service phân tích và chấm điểm phân khúc khách hàng theo RFM.
/// </summary>
public interface IRfmService
{
    /// <summary>
    /// Tính toán phân khúc RFM dựa trên 3 chỉ số R, F, M.
    /// </summary>
    /// <param name="recencyDays">Số ngày kể từ lần ghé gần nhất</param>
    /// <param name="frequency">Tổng số lần ghé salon</param>
    /// <param name="monetary">Tổng số tiền chi tiêu (VNĐ)</param>
    /// <returns>Phân khúc RfmSegment tương ứng</returns>
    RfmSegment ClassifyCustomer(int recencyDays, int frequency, decimal monetary);

    /// <summary>
    /// Tạo đối tượng RfmScore đầy đủ cho khách hàng.
    /// </summary>
    RfmScore ComputeRfmScore(int customerId, int recencyDays, int frequency, decimal monetary);
}
