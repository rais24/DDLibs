using Dapper;
using System.Data;
using Utils.IRepository;
using Utils.Models;

namespace Utils.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class LoginRepository : ILoginRepository
    {
        private readonly UtilsDbContext _context;

        /// <inheritdoc/>
        public LoginRepository(UtilsDbContext context)
        {
            _context = context;
        }
        /// <inheritdoc/>
        public async Task ExpireOTP(string useremail, DateTime createdDate)
        {
            DateTime expire_datetime = createdDate.AddMinutes(-10);
            string query = " update login_otp set created_date = @crdate where email = @useremail; ";

            DynamicParameters parameter = new();
            parameter.Add("@useremail", useremail, DbType.String);
            parameter.Add("@crdate", expire_datetime, DbType.DateTime);

            using (var connection = _context.CreateConnection())
            {
                await connection.QueryAsync(query, parameter);
            }
        }

        /// <inheritdoc/>
        public async Task<int> Add(OTPModel entity)
        {
            int? otpId = 0;
            string query = "if exists(select id from login_otp where email = @email) begin update login_otp set otp = @otp, created_date = getutcdate() where email = @email end else begin insert into login_otp (email,otp,created_date) values (@email,@otp,GETUTCDATE()); end SELECT SCOPE_IDENTITY();";

            DynamicParameters parameters = new();
            parameters.Add("@email", entity.Email, DbType.String);
            parameters.Add("@otp", entity.OTP, DbType.String);

            using (var connection = _context.CreateConnection())
            {
                otpId = await connection.QuerySingleOrDefaultAsync<int?>(query, parameters);
            }

            return otpId ?? 0;

        }

        /// <inheritdoc/>
        public async Task<OTPModel> GetByVar(dynamic arg)
        {
            OTPModel entity = new();

            string query = "select otp,created_date from login_otp where email = @email";

            DynamicParameters parameters = new();
            parameters.Add("@email", arg, DbType.String);

            using (var connection = _context.CreateConnection())
            {
                var data = await connection.QuerySingleOrDefaultAsync<(string otp, DateTime created_date)>(query, parameters);
                entity.Email = arg;
                entity.OTP = data.otp;
                entity.CreatedDate = data.created_date;
            }

            return entity;
        }

        /// <inheritdoc/>
        public async Task<string> IsUserExist(string useremail)
        {
            string userName = string.Empty;
            string query = "select user_id,user_first_name from user_main where user_email = @email and user_status = 1 and is_deleted is null";

            DynamicParameters parameters = new();
            parameters.Add("@email", useremail, DbType.String);

            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QuerySingleOrDefaultAsync<(int? user_id, string user_first_name)>(query, parameters);

                if (user.user_id != 0 && user.user_id != null)
                    userName = user.user_first_name;

            }

            return userName;
        }

        /// <inheritdoc/>
        // queries are for reference purpose, you can update them according to your need.
        public async Task<bool> UpdatePassword(string useremail, string password)
        {
            bool result = true;

            string query = "update user_main set password = @pass where user_email = @useremail";

            DynamicParameters parameters = new();
            parameters.Add("@pass", password);
            parameters.Add("@useremail", useremail);

            using (var connection = _context.CreateConnection())
            {
                try
                {
                    await connection.QueryAsync(query, parameters);
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }
        /// <inheritdoc/>
        public async Task<Dictionary<string, string>> ValidateLogin(string useremail, string password)
        {
            Dictionary<string, string> userData = new();
            string query = "select u.user_id,u.requestor_id_ref,r.user_role_name from user_main u join user_role_main r on u.user_role_id_ref = r.user_role_id where user_email = @email and password = @password and user_status = 1";

            DynamicParameters parameters = new();
            parameters.Add("@email", useremail, DbType.String);
            parameters.Add("@password", password, DbType.String);

            // here dapper is being used to query the database.

            using (var connection = _context.CreateConnection())
            {
                var user = await connection.QuerySingleOrDefaultAsync<(int user_id, int? requestor_id_ref, string? user_role_name)>(query, parameters);
                userData.Add("user_id", Convert.ToString(user.user_id));
                userData.Add("requestor_id", Convert.ToString(user.requestor_id_ref ?? 0));
                userData.Add("role", user.user_role_name?? String.Empty);

            }

            return userData;
        }
    }
}
