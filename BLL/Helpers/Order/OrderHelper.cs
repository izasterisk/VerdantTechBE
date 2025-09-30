using BLL.DTO.Order;

namespace BLL.Helpers.Order;

public class OrderHelper
{
    /// <summary>
    /// Tính tổng tiền đơn hàng cuối cùng.
    /// </summary>
    public static decimal ComputeTotalAmountForOrder(decimal subtotal, decimal taxAmount, decimal shippingFee, decimal discountAmount)
    {
        return subtotal + taxAmount + shippingFee - discountAmount;
    }
    
    public static decimal ComputeSubtotalForOrderItem(int quantity, decimal unitPrice, decimal discountAmount)
    {
        return quantity * unitPrice - discountAmount;
    }
    
    public static bool AreOrderDetailsEqual(OrderDetailUpdateDTO a, OrderDetailUpdateDTO b)
    {

        if (a.Quantity.HasValue && b.Quantity.HasValue && a.Quantity.Value != b.Quantity.Value)
            return false;

        if (a.UnitPrice.HasValue && b.UnitPrice.HasValue && a.UnitPrice.Value != b.UnitPrice.Value)
            return false;

        if (a.DiscountAmount.HasValue && b.DiscountAmount.HasValue && a.DiscountAmount.Value != b.DiscountAmount.Value)
            return false;

        return true;
    }

}