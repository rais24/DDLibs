using Utils.Models;

namespace Utils.IRepository
{
    public interface ILoginRepository : IBaseRepository<OTPModel>
    {
        Task<Dictionary<string, string>> ValidateLogin(string useremail, string password);
        public Task ExpireOTP(string useremail, DateTime createdDate);
        public Task<bool> UpdatePassword(string useremail, string password);
        public Task<string> IsUserExist(string useremail);
    }
}
