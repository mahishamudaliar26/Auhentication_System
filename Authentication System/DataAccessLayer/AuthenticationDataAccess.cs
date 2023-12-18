using Authentication_System.Model;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authentication_System.DataAccessLayer
{
    public class AuthenticationDataAccess : IAuthenticationDataAccess
    {
        private readonly IConfiguration _configuration;
        private readonly SqlConnection _mySqlConnection;

        public AuthenticationDataAccess(IConfiguration configuration)
        {
            _configuration = configuration;
            _mySqlConnection = new SqlConnection(_configuration["ConnectionStrings:MySqlConnection"]);
        }

        public async Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request)
        {
            RegisterUserResponse response = new RegisterUserResponse
            {
                IsSuccess = true,
                Message = "Successful"
            };

            try
            {
                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                string sqlQuery = @"INSERT INTO userdetail (UserName, PassWord, Role) 
                                    VALUES (@UserName, @PassWord, @Role);";

                int status = await _mySqlConnection.ExecuteAsync(sqlQuery, request);

                if (status <= 0)
                {
                    response.IsSuccess = false;
                    response.Message = "Register Query Not Executed";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _mySqlConnection.CloseAsync();
            }

            return response;
        }

        public async Task<UserLoginResponse> UserLogin(UserLoginRequest request)
        {
            UserLoginResponse response = new UserLoginResponse
            {
                IsSuccess = true,
                Message = "Successful"
            };

            try
            {
                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                string sqlQuery = @"SELECT UserId, UserName, Role
                                    FROM userdetail
                                    WHERE UserName=@UserName AND PassWord=@Password";

                var result = await _mySqlConnection.QuerySingleOrDefaultAsync<UserLoginInformation>(sqlQuery, request);

                if (result != null)
                {
                    response.Message = "Login Successfully";
                    response.data = result;
                    response.Token = GenerateJWT(result.UserID, result.UserName, result.Role);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Login Unsuccessful";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _mySqlConnection.CloseAsync();
            }

            return response;
        }

        public async Task<AddInformationResponse> AddInformation(AddInformationRequest request)
        {
            AddInformationResponse response = new AddInformationResponse
            {
                IsSuccess = true,
                Message = "Successful"
            };

            try
            {
                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                string sqlQuery = @"INSERT INTO crudapplication (UserName, EmailID, MobileNumber, Salary, Gender) 
                                    VALUES (@UserName, @EmailID, @MobileNumber, @Salary, @Gender)";

                int status = await _mySqlConnection.ExecuteAsync(sqlQuery, request);

                if (status <= 0)
                {
                    response.IsSuccess = false;
                    response.Message = "AddInformation Query Not Executed";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _mySqlConnection.CloseAsync();
            }

            return response;
        }

        public async Task<GetInformationResponse> GetInformation()
        {
            GetInformationResponse response = new GetInformationResponse
            {
                IsSuccess = true,
                Message = "Successful"
            };

            try
            {
                if (_mySqlConnection.State != System.Data.ConnectionState.Open)
                {
                    await _mySqlConnection.OpenAsync();
                }

                string sqlQuery = @"SELECT * FROM crudapplication";

                response.getInformation = (List<GetInformation>)await _mySqlConnection.QueryAsync<GetInformation>(sqlQuery);

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            finally
            {
                await _mySqlConnection.CloseAsync();
            }

            return response;
        }

        private string GenerateJWT(string UserId, string Email, string Role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sid, UserId),
                new Claim(JwtRegisteredClaimNames.Email, Email),
                new Claim(ClaimTypes.Role, Role),
                new Claim("Date", DateTime.Now.ToString()),
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
