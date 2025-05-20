using CustomMiddleWare.Interfaces;
using CustomMiddleWare.Models;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;

namespace CustomMiddleWare.Services
{
    public class RegistrationService : IRegistration
    {
        private readonly IConfiguration _config;
        private readonly IDbConnection _connection;

        public RegistrationService(IConfiguration config, IDbConnection connection)
        {
            _config = config;
            _connection = connection;
        }

        public async Task<ResultModel<object>> Add(RegistrationModel oRegistration)
        {
            ResultModel<object> resultModel = new ResultModel<object>();

            try
            {
                if (oRegistration != null)
                {
                    var sql = @"INSERT INTO registration (firstname, lastname, email, phone, address, city, country, postalcode) VALUES (@firstname, @lastname, @email, @phone, @address, @city, @country, @postalcode);";
                    var rowAffected = await _connection.ExecuteAsync(sql, oRegistration);
                    resultModel.success = rowAffected > 0;
                    resultModel.message = rowAffected > 0 ? "Registration successful" : "Registration failed";
                }
            } catch (Exception ex)
            {
                resultModel.success = false;
                resultModel.message = $"Error: {ex.Message}";
            }
            return resultModel;
        }

        public async Task<ResultModel<object>> LoginUser(LoginModel oLoginModel)
        {
            ResultModel<object> resultModel = new ResultModel<object>();
            try
            {
                if(oLoginModel != null)
                {
                    var sql = "SELECT * FROM registration WHERE firstname = @firstname and email = @email";
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    dynamicParameters.Add("firstname", oLoginModel.firstname, DbType.String);
                    dynamicParameters.Add("email", oLoginModel.email, DbType.String);

                    var count = await _connection.QueryAsync<LoginModel>(sql, dynamicParameters);
                    var countData = await _connection.QueryFirstOrDefaultAsync<int>(sql, dynamicParameters);

                    resultModel.success = countData > 0;
                    resultModel.data = count.ToList();
                } else
                {
                    resultModel.success = false;
                    resultModel.message = "Login failed";
                    resultModel.error = true;
                }
            } catch (Exception ex)
            {
                throw ex;
            }

            return resultModel;
        }


        public async Task<ResultModel<RegistrationModel>> GetHashCode(string email)
        {
            ResultModel<RegistrationModel> result = new ResultModel<RegistrationModel>();
            try
            {
                if (email != null)
                {
                    var sql = "SELECT * FROM registration WHERE email = @email";
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    dynamicParameters.Add("email", email);

                    var count = await _connection.QueryAsync<RegistrationModel>(sql, dynamicParameters);
                    var countData = await _connection.QueryFirstOrDefaultAsync(sql, dynamicParameters);
                    if(countData > 0)
                    {
                        result.success = countData > 0;
                        result.LstModel = count.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
    }
}
