using SalonCare.Core.Entities;
using SalonCare.Core.Enums;
using SalonCare.Core.Interfaces;

namespace SalonCare.Core.Services;

/// <summary>
/// Hiện thực Service tính toán và phân loại khách hàng theo mô hình RFM.
/// Hỗ trợ salon nhận diện sớm khách hàng có nguy cơ rời bỏ (AtRisk / Inactive > 60 ngày)
/// để gửi kịch bản Zalo ZNS giữ chân và giảm tỷ lệ No-show.
/// </summary>
public class RfmService : IRfmService
{
    /// <inheritdoc />
    public RfmSegment ClassifyCustomer(int recencyDays, int frequency, decimal monetary)
    {
        // Khách hàng không hoạt động trên 120 ngày -> Khách đã mất
        if (recencyDays > 120)
        {
            return RfmSegment.Lost;
        }

        // Khách hàng ngưng hoạt động trên 60 ngày -> Nguy cơ rời bỏ (AtRisk)
        if (recencyDays > 60)
        {
            return RfmSegment.AtRisk;
        }

        // Khách hàng ghé trong vòng 30 ngày, tần suất >= 5 lần, chi tiêu >= 2.000.000 VNĐ -> Khách VIP (Champions)
        if (recencyDays <= 30 && frequency >= 5 && monetary >= 2_000_000m)
        {
            return RfmSegment.Champions;
        }

        // Khách hàng ghé trong vòng 45 ngày, tần suất >= 3 lần -> Khách thân thiết (Loyal)
        if (recencyDays <= 45 && frequency >= 3)
        {
            return RfmSegment.Loyal;
        }

        // Còn lại là nhóm Khách tiềm năng (Potential Loyalist)
        return RfmSegment.PotentialLoyalist;
    }

    /// <inheritdoc />
    public RfmScore ComputeRfmScore(int customerId, int recencyDays, int frequency, decimal monetary)
    {
        var segment = ClassifyCustomer(recencyDays, frequency, monetary);

        return new RfmScore
        {
            CustomerId = customerId,
            RecencyDays = recencyDays,
            Frequency = frequency,
            Monetary = monetary,
            RfmSegment = segment,
            CalculatedAt = DateTime.UtcNow
        };
    }
}
