namespace BLL.Helpers.VendorBankAccounts;

public class VendorBankAccountsHelper
{
    /// <summary>
    /// Validate BankCode có đúng dạng số hay không
    /// </summary>
    /// <param name="bankCode">Mã ngân hàng dạng string</param>
    /// <exception cref="ArgumentException">Ném ra khi BankCode không phải là số hợp lệ</exception>
    public static void ValidateBankCode(string bankCode)
    {
        if (!int.TryParse(bankCode, out _))
            throw new ArgumentException("BankCode không hợp lệ.");
    }
}