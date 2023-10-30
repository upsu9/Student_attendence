using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Student_attendence.AuthFile;
using Student_attendence.Data;
using Student_attendence.Models;
using Student_attendence.Models.ModelDto;
using Student_attendence.RequestModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Student_attendence.Controllers
{
    [Route("api/[controller]")]
   // [Route("api/studentAPI")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly JwtAuthentication _jwtAuthentication;

        public StudentController(ApplicationDbContext db, JwtAuthentication jwtAuthentication)
        {
            _db = db;
            _jwtAuthentication = jwtAuthentication;

        }

        [Authorize]
        [HttpGet]
        [Route("/all-registered-student")]
        public async Task<IActionResult> GetRegAllStudent()
        {
            return Ok(_db.StudentMasterData.Where(student => student.UniqueId != null && student.IsActive == IsActiveEnum.Active).Select(user => user).ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("/all-unregistered-student")]
        public async Task<IActionResult> GetUnregAllStudent()
        {
            return Ok(_db.StudentMasterData.Where(student => student.UniqueId == null  && student.IsActive == IsActiveEnum.Active).Select(user => user).ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("/all-user")]
        public async Task<IActionResult> GetAllUser()
        {
            return Ok(_db.AdminUserData.Where(user => user.IsActive == IsActiveEnum.Active).Select(user => user).ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("/get-single-user-by-id")]
        public async Task<IActionResult> GetSingleUser(string uniqueId)
        {
            return Ok(_db.AdminUserData.Where(user => user.UniqueId.Trim().ToLower() == uniqueId.Trim().ToLower() && user.IsActive == IsActiveEnum.Active).Select(user => user).ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("/get-single-student-by-id")]
        public async Task<IActionResult> GetSingleStudentById(string uniqueId)
        {
            return Ok(_db.StudentMasterData.Where(user => user.UniqueId.Trim().ToLower() == uniqueId.Trim().ToLower() && user.IsActive == IsActiveEnum.Active).Select(user => user).ToList());
        }

        [Authorize]
        [HttpGet]
        [Route("/get-single-student-by-name")]
        public async Task<IActionResult> GetSingleStudentByName(string studentName)
        {
            return Ok(_db.StudentMasterData.Where(user => user.StudentName.Trim().ToLower() == studentName.Trim().ToLower() && user.IsActive == IsActiveEnum.Active).Select(user => user).ToList());
        }

        [Authorize]
        [HttpPost]
        [Route("/add-student")]
        public ActionResult<Response> PostStudent([FromBody]Student studentData)
        {
            var response = new Response();
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

                studentData.IsActive = IsActiveEnum.Active;
                studentData.CreatedAt = indiaTime;
                _db.StudentMasterData.Add(studentData);
                _db.SaveChanges();
                response.IsSuccess = true;
                response.ResponseMessage = "Data is added";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("/add-user")]
        public async Task<ActionResult<Response>> PostAdminUser([FromBody] List<AdminUser> adminUserData)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            if((AdminUserStatusEnum)Enum.Parse(typeof(AdminUserStatusEnum), userStatus) != AdminUserStatusEnum.Admin)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "You dont have rights.";
                response.Result = null;
                return response;
            }
            try
            {
                List<AdminUser> postNewAdminUser = new List<AdminUser>();
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                foreach (var firstObj in adminUserData)
                {
                    var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(firstObj.Password);
                    firstObj.Password = encryptedPassword;
                    firstObj.CreatedBy = userId;
                    firstObj.IsActive = IsActiveEnum.Active;
                    firstObj.CreatedAt = indiaTime;
                }
                postNewAdminUser.AddRange(adminUserData);
                await _db.AdminUserData.AddRangeAsync(postNewAdminUser);
                await _db.SaveChangesAsync();
                response.IsSuccess = true;
                response.ResponseMessage = "Data is added";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPut]
        [Route("/edit-self-user")]
        public async Task<ActionResult<Response>> EditSelfAdminUser([FromBody] List<EditUser> adminUserData)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                AdminUser postNewAdminUser = new AdminUser();
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                foreach (var firstObj in adminUserData)
                {
                    var ifUserExist = _db.AdminUserData.FirstOrDefault(user => user.AdminUserId == Convert.ToInt32(firstObj.EditedAdminUserId) && user.IsActive == IsActiveEnum.Active);
                    if (ifUserExist is null)
                    {
                        response.IsSuccess = false;
                        response.ResponseMessage = "User does not exist.";
                        response.Result = null;
                        return response;
                    }
                    ifUserExist.FatherName = firstObj.FatherName;
                    ifUserExist.PhoneNumber = firstObj.PhoneNumber;
                    ifUserExist.UniqueId = firstObj.UniqueId;
                    ifUserExist.AdminUserName = firstObj.AdminUserName;
                    ifUserExist.Branch = firstObj.Branch;
                    ifUserExist.EmailId = firstObj.EmailId;
                    ifUserExist.Gender = firstObj.Gender;
                   // var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(firstObj.Password);
                    //ifUserExist.Password = encryptedPassword;
                    ifUserExist.UpdatedBy = userId;
                    ifUserExist.Status = (AdminUserStatusEnum)Enum.Parse(typeof(AdminUserStatusEnum), userStatus) != AdminUserStatusEnum.Admin ? AdminUserStatusEnum.User : firstObj.Status;
                    firstObj.IsActive = IsActiveEnum.Active;
                    ifUserExist.UpdatedAt = indiaTime;

                    await _db.SaveChangesAsync();
                    response.IsSuccess = true;
                    response.ResponseMessage = "Edited Data is added";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPut]
        [Route("/delete-user")]
        public async Task<ActionResult<Response>> DeleteUser([FromBody] int deleteUserId)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                AdminUser postNewAdminUser = new AdminUser();
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                    var ifUserExist = _db.AdminUserData.FirstOrDefault(user => user.AdminUserId == userId && user.IsActive == IsActiveEnum.Active);
                    if (ifUserExist is null)
                    {
                        response.IsSuccess = false;
                        response.ResponseMessage = "User does not exist.";
                        response.Result = null;
                        return response;
                    }
                    ifUserExist.UpdatedBy = userId;
                    ifUserExist.IsActive = IsActiveEnum.Inactive;
                    ifUserExist.UpdatedAt = indiaTime;

                    await _db.SaveChangesAsync();
                    response.IsSuccess = true;
                    response.ResponseMessage = "User is Deleted.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPut]
        [Route("/forgot-password")]
        public async Task<ActionResult<Response>> ForgotPassword([FromBody] string emailId, string password)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                AdminUser postNewAdminUser = new AdminUser();
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                var ifUserExist = _db.AdminUserData.FirstOrDefault(user => user.EmailId.ToLower() == emailId.Trim().ToLower() && user.IsActive == IsActiveEnum.Active);
                if (ifUserExist is null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "User does not exist.";
                    response.Result = null;
                    return response;
                }
                var encryptedPassword = BCrypt.Net.BCrypt.HashPassword(password.Trim().ToLower());
                ifUserExist.Password = encryptedPassword;
                ifUserExist.UpdatedBy = userId;
                ifUserExist.IsActive = IsActiveEnum.Active;
                ifUserExist.UpdatedAt = indiaTime;

                await _db.SaveChangesAsync();
                response.IsSuccess = true;
                response.ResponseMessage = "Password is changed.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("/mark-registered-user")]
        public async Task<Response> PostRegisteredUser([FromBody] MarkRegAttendence markRegAttendence)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                //DateTime dateNow = DateTime.ParseExact(markRegAttendence.date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                DateTime dateTimeWithTime = DateTime.ParseExact(markRegAttendence.date, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                DateTime dateOnly = dateTimeWithTime.Date;

                if (dateTimeWithTime > indiaTime)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "Future Attendence not allowed.";
                    return response;
                }
                var checkIfValidUser = _db.StudentMasterData.Where(st => st.UniqueId.Trim().ToLower() == markRegAttendence.uniqueId.Trim().ToLower() && st.IsActive == IsActiveEnum.Active).FirstOrDefault();
                if(checkIfValidUser is null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "user is not valid.";
                    return response;
                }
                var checkIfUserMarkedAtSameTime = _db.RegisteredAttendenceData.Where(id => id.IsActive == IsActiveEnum.Active &&
                id.DayStatus == (DayStatusEnum)Enum.Parse(typeof(DayStatusEnum), markRegAttendence.dayStatus) &&
                id.StudentId == checkIfValidUser.StudentId && id.AttendenceDate.Date == dateOnly).FirstOrDefault();
                if (checkIfUserMarkedAtSameTime is not null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "attendence is already marked.";
                    return response;
                }
                RegisteredAttendence postAttendence = new RegisteredAttendence();
                postAttendence.StudentId = checkIfValidUser.StudentId;
                postAttendence.AttendenceDate = dateTimeWithTime;
                postAttendence.CreatedAt = indiaTime;
                postAttendence.CreatedBy = userId;
                postAttendence.DayStatus = (DayStatusEnum)Enum.Parse(typeof(DayStatusEnum), markRegAttendence.dayStatus);
                postAttendence.IsActive = IsActiveEnum.Active;
                _db.RegisteredAttendenceData.Add(postAttendence);
                _db.SaveChanges();
                response.IsSuccess = true;
                response.ResponseMessage = "attendence is marked";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPut]
        [Route("/edit-mark-registered-user")]
        public async Task<Response> EditPostRegisteredUser([FromBody] MarkRegAttendence markRegAttendence)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                DateTime dateNow = DateTime.ParseExact(markRegAttendence.date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                DateTime dateOnly = dateNow.Date;
                var checkIfValidUser = _db.StudentMasterData.Where(st => st.UniqueId.Equals(markRegAttendence.uniqueId) && st.IsActive == IsActiveEnum.Active).FirstOrDefault();
                if (checkIfValidUser is null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "user is not valid.";
                    return response;
                }
                var checkIfUserMarkedAtSameTime = _db.RegisteredAttendenceData.Where(id => id.IsActive == IsActiveEnum.Active &&
                id.DayStatus == (DayStatusEnum)Enum.Parse(typeof(DayStatusEnum), markRegAttendence.dayStatus) &&
                id.StudentId == checkIfValidUser.StudentId && id.CreatedAt == dateOnly).FirstOrDefault();
                if (checkIfUserMarkedAtSameTime is null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "attendence is not marked.";
                }
                checkIfUserMarkedAtSameTime.StudentId = checkIfValidUser.StudentId;
                checkIfUserMarkedAtSameTime.UpdatedAt = indiaTime;
                checkIfUserMarkedAtSameTime.UpdatedBy = userId;
                checkIfUserMarkedAtSameTime.DayStatus = (DayStatusEnum)Enum.Parse(typeof(DayStatusEnum), markRegAttendence.dayStatus);
                checkIfUserMarkedAtSameTime.IsActive = IsActiveEnum.Active;
                _db.SaveChanges();
                response.IsSuccess = true;
                response.ResponseMessage = "attendence is marked";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("/mark-unregistered-user")]
        public async Task<Response> PostUnRegisteredUser([FromBody] MarkUnRegAttendence markUnRegAttendence)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                DateTime dateNow = DateTime.ParseExact(markUnRegAttendence.date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                DateTime dateOnly = dateNow.Date;
                
                UnRegisteredAttendence postAttendence = new UnRegisteredAttendence();
                postAttendence.StudentName = markUnRegAttendence.studentName;
                postAttendence.DayStatus = (DayStatusEnum)Enum.Parse(typeof(DayStatusEnum), markUnRegAttendence.dayStatus);
                postAttendence.CreatedAt = indiaTime;
                postAttendence.CreatedBy = userId;
                postAttendence.IsActive = IsActiveEnum.Inactive;
                _db.UnRegisteredAttendenceData.Add(postAttendence);
                _db.SaveChanges();
                response.IsSuccess = true;
                response.ResponseMessage = "attendence is marked";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPut]
        [Route("/edit-mark-unregistered-user")]
        public async Task<Response> EditPostUnRegisteredUser([FromBody] MarkUnRegAttendence markUnRegAttendence)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                DateTime dateNow = DateTime.ParseExact(markUnRegAttendence.date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                DateTime dateOnly = dateNow.Date;
                var checkIfUserMarkedAtSameTime = _db.UnRegisteredAttendenceData.Where(id => id.IsActive == IsActiveEnum.Active &&
                id.DayStatus == (DayStatusEnum)Enum.Parse(typeof(DayStatusEnum), markUnRegAttendence.dayStatus) &&
                id.StudentName.Trim().ToLower() == markUnRegAttendence.studentName.Trim().ToLower() && id.CreatedAt == dateOnly).FirstOrDefault();
                if (checkIfUserMarkedAtSameTime is null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "attendence is not marked.";
                }
                checkIfUserMarkedAtSameTime.StudentName = markUnRegAttendence.studentName.Trim();
                checkIfUserMarkedAtSameTime.UpdatedAt = indiaTime;
                checkIfUserMarkedAtSameTime.UpdatedBy = userId;
                checkIfUserMarkedAtSameTime.IsActive = IsActiveEnum.Active;
                _db.SaveChanges();
                response.IsSuccess = true;
                response.ResponseMessage = "attendence is marked";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("/get-excel-period-report-by-id")]
        public async Task<Response> GetExcelReportById([FromBody]ExcelReportById excelReportModel)
        {
            var response = new Response();
            try
            {
                var fetchUserDetails = _db.StudentMasterData.Where(user => user.UniqueId.Trim().ToLower() == excelReportModel.UniqueId.Trim().ToLower() && user.IsActive == IsActiveEnum.Active).FirstOrDefault();
                if (fetchUserDetails is null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "User not exist.";
                    return response;
                }

                var getAllData = await GetUserAttendenceData(excelReportModel, fetchUserDetails.StudentId);
                if (!getAllData.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = getAllData.ResponseMessage;
                    return response;
                }
                else
                {
                    var alluserattendenceData = new List<ExcelAllResponseModel>();
                    var serializedData = JsonConvert.SerializeObject(getAllData.Result);
                    var authData = JsonConvert.DeserializeObject<List<ExcelAllResponseModel>>(serializedData);
                    if (authData.Count() == 0)
                    {
                        response.IsSuccess = false;
                        response.ResponseMessage = "Problem while fetching userData result.";
                        return response;
                    }

                    DataTable dt = new DataTable("Grid");
                    dt.Columns.AddRange(new DataColumn[6] { new DataColumn("StudentName"),
                                     new DataColumn("DayStatus") ,
                                     new DataColumn("Unique Number") ,
                                     new DataColumn("Attendence Date") ,
                                     new DataColumn("Attendence At"),
                                     new DataColumn("Attendence By"),
                });
                    foreach (var emp in authData)
                    {
                        dt.Rows.Add(emp.StudentName, emp.DayStatus, emp.StudentUniqueNumber,emp.AttendenceDate.ToString(), emp.CreatedAt.ToString(), emp.CreatedBy);
                    }
                    //using ClosedXML.Excel;

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt);
                        using (MemoryStream stream = new MemoryStream())
                        {
                            wb.SaveAs(stream);
                            response.IsSuccess = true;
                            response.ResponseMessage = "Excel Generated";
                            response.Result = File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendenceSheet.xlsx");
                            return response;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "Error while fetching.";
                response.Result = ex;
                return response;
            }
        }

        [Authorize]
        [HttpGet]
        [Route("/get-excel-period-report-for-all")]
        public async Task<Response> GetExcelReportForAll(string fromDate, string tillDate)
        {
            var response = new Response();
            var getAllData = await GetAllUserAttendenceData(fromDate, tillDate);
            if (!getAllData.IsSuccess)
            {
                response.IsSuccess = false;
                response.ResponseMessage = getAllData.ResponseMessage;
                return response;
            }
            else
            {
                var alluserattendenceData = new List<ExcelAllResponseModel>();
                var serializedData = JsonConvert.SerializeObject(getAllData.Result);
                var authData = JsonConvert.DeserializeObject<List<ExcelAllResponseModel>>(serializedData);
                if (authData.Count() == 0)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "Problem while fetching userData result.";
                    return response;
                }
                DataTable dt = new DataTable("Grid");
                dt.Columns.AddRange(new DataColumn[6] { new DataColumn("StudentName"),
                                     new DataColumn("DayStatus") ,
                                     new DataColumn("Unique Number") ,
                                     new DataColumn("Attendence Date") ,
                                     new DataColumn("Attendence At"),
                                     new DataColumn("Attendence By"),
                });
                foreach (var emp in authData)
                {
                    dt.Rows.Add(emp.StudentName, emp.DayStatus,emp.StudentUniqueNumber, emp.AttendenceDate.ToString(), emp.CreatedAt.ToString(), emp.CreatedBy);
                }
                //using ClosedXML.Excel;

                using (XLWorkbook wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        response.IsSuccess = true;
                        response.ResponseMessage = "Excel Generated";
                        response.Result = File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AttendenceSheet.xlsx");
                        return response;
                    }
                }
            }
        }

        private async Task<Response> GetUserAttendenceData(ExcelReportById excelReportModel, int studentId)
        {
            var response = new Response();
            DateTime FromdateNow = DateTime.ParseExact(excelReportModel.FromDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            DateTime FromdateOnly = FromdateNow.Date;
            DateTime TilldateNow = DateTime.ParseExact(excelReportModel.TillDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            DateTime TilldateOnly = TilldateNow.Date;
            if (TilldateOnly < FromdateOnly)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "From Date cannot be lesser than till date.";
                return response;
            }
            var alluserattendenceData = new List<ExcelAllResponseModel>();
            var fetchuserattendenceData = new List<ExcelAllResponseModel>();
            if (excelReportModel.DayStatus == "all")
            {
                alluserattendenceData = _db.RegisteredAttendenceData.Join(
                                    _db.StudentMasterData,
                                    t1 => t1.StudentId,
                                    t2 => t2.StudentId,
                                    (t1, t2) => new { Table1 = t1, Table2 = t2 }
                                )
                             .Where(user =>
                             user.Table1.StudentId.Equals(studentId) &&
                             user.Table1.IsActive == IsActiveEnum.Active &&
                             user.Table2.IsActive == IsActiveEnum.Active).Select(user =>
                             new ExcelAllResponseModel()
                             {
                                 StudentId = user.Table1.StudentId,
                                 StudentName = user.Table2.StudentName,
                                 StudentUniqueNumber = user.Table2.UniqueId,
                                 AttendenceDate = user.Table1.AttendenceDate,
                                 CreatedAt = user.Table1.CreatedAt,
                                 CreatedBy = user.Table1.CreatedBy,
                                 DayStatus = user.Table1.DayStatus
                             }
                             ).ToList();
            }
            else
            {
             
                alluserattendenceData = _db.RegisteredAttendenceData.Join(
                                    _db.StudentMasterData,
                                    t1 => t1.StudentId,
                                    t2 => t2.StudentId,
                                    (t1, t2) => new { Table1 = t1, Table2 = t2 }
                                )
                             .Where(user =>
                             user.Table1.StudentId.Equals(studentId) &&
                             user.Table1.IsActive == IsActiveEnum.Active &&
                             user.Table2.IsActive == IsActiveEnum.Active &&
                             user.Table1.DayStatus == (DayStatusEnum)Enum.Parse(typeof(DayStatusEnum), excelReportModel.DayStatus)
                             ).Select(user =>
                             new ExcelAllResponseModel()
                             {
                                 StudentId = user.Table1.StudentId,
                                 StudentName = user.Table2.StudentName,
                                 StudentUniqueNumber = user.Table2.UniqueId,
                                 AttendenceDate = user.Table1.AttendenceDate,
                                 CreatedAt = user.Table1.CreatedAt,
                                 CreatedBy = user.Table1.CreatedBy,
                                 DayStatus = user.Table1.DayStatus
                             }
                             ).ToList();
            }

            if (alluserattendenceData.Count > 0)
            {
                foreach(var user in alluserattendenceData)
                {
                        if (user.AttendenceDate.Date >= FromdateOnly && user.AttendenceDate.Date <= TilldateOnly)
                        {
                            fetchuserattendenceData.Add(user);
                        }
                }
                response.IsSuccess = fetchuserattendenceData.Count > 0 ? true : false;
                response.ResponseMessage = fetchuserattendenceData.Count > 0 ? "user found." : "No user found.";
                response.Result = fetchuserattendenceData;
            }
            else
            {
                response.IsSuccess = false;
                response.ResponseMessage = "No user found.";
            }
            return response;
        }

        private async Task<Response> GetAllUserAttendenceData(string FromDate, string TillDate)
        {
            var response = new Response();
            DateTime FromdateNow = DateTime.ParseExact(FromDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            DateTime FromdateOnly = FromdateNow.Date;
            DateTime TilldateNow = DateTime.ParseExact(TillDate, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            DateTime TilldateOnly = TilldateNow.Date;
            if(TilldateOnly < FromdateOnly)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "From Date cannot be lesser than till date.";
                return response;
            }
            var fetchuserattendenceData = new List<ExcelAllResponseModel>();
            //var alluserattendenceData = (from t1 in _db.RegisteredAttendenceData
            //                         join t2 in _db.StudentMasterData on t1.StudentId equals t2.StudentId
            //                         where t1.IsActive == t2.IsActive
            //                         select new { t1, t2 }).ToList();
            var alluserattendenceData = _db.RegisteredAttendenceData.Join(
                                    _db.StudentMasterData,
                                    t1 => t1.StudentId,
                                    t2 => t2.StudentId,
                                    (t1, t2) => new { Table1 = t1, Table2 = t2 }
                                )
                             .Where(user => 
                             user.Table1.IsActive == IsActiveEnum.Active  &&
                             user.Table2.IsActive == IsActiveEnum.Active).Select(user =>
                             new ExcelAllResponseModel()
                             {
                                StudentId = user.Table1.StudentId,
                               StudentName = user.Table2.StudentName,
                               StudentUniqueNumber = user.Table2.UniqueId,
                               AttendenceDate = user.Table1.AttendenceDate,
                               CreatedAt = user.Table1.CreatedAt,
                                CreatedBy =  user.Table1.CreatedBy,
                               DayStatus = user.Table1.DayStatus
                             }
                             ).ToList();

            if (alluserattendenceData.Count > 0)
            {
                foreach (var user in alluserattendenceData)
                {
                    if (user.AttendenceDate.Date >= FromdateOnly && user.AttendenceDate.Date <= TilldateOnly)
                    {
                        fetchuserattendenceData.Add(user);
                    }
                }
                response.IsSuccess = fetchuserattendenceData.Count > 0 ? true : false;
                response.ResponseMessage = fetchuserattendenceData.Count > 0 ? "user found." : "No user found.";
                response.Result = fetchuserattendenceData;
            }
            else
            {
                response.IsSuccess = false;
                response.ResponseMessage = "No user found.";
            }
            return response;
        }

        [Authorize]
        [HttpGet]
        [Route("/get-caledar-data")]
        public async Task<Response> GetCalendarData(string date)
        {
            var response = new Response();
            DateTime dateNow = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            DateTime dateNowOnly = dateNow.Date;
            var alluserattendenceData = _db.RegisteredAttendenceData.Join(
                                    _db.StudentMasterData,
                                    t1 => t1.StudentId,
                                    t2 => t2.StudentId,
                                    (t1, t2) => new { Table1 = t1, Table2 = t2 }
                                )
                             .Where(user =>
                             user.Table1.IsActive == IsActiveEnum.Active &&
                             user.Table2.IsActive == IsActiveEnum.Active &&
                             user.Table1.AttendenceDate.Date == dateNowOnly).Select(user =>
                             new ExcelAllResponseModel()
                             {
                                 StudentId = user.Table1.StudentId,
                                 StudentName = user.Table2.StudentName,
                                 StudentUniqueNumber = user.Table2.UniqueId,
                                 AttendenceDate = user.Table1.AttendenceDate,
                                 CreatedAt = user.Table1.CreatedAt,
                                 CreatedBy = user.Table1.CreatedBy,
                                 DayStatus = user.Table1.DayStatus
                             }
                             ).ToList();
            if (!alluserattendenceData.Any())
            {
                response.IsSuccess = false;
                response.ResponseMessage = "No user found.";
                return response;
            }
            else
            {
                int morningData = 0;
                int eveningData = 0;
                foreach (var user in alluserattendenceData)
                {
                    
                    if ( user.DayStatus == DayStatusEnum.Morning)
                    {
                        morningData = morningData + 1;
                    }
                    else
                    {
                        eveningData = eveningData + 1;
                    }
                }
                int total = morningData + eveningData;
                
                response.IsSuccess = true;
                response.ResponseMessage = "user Found";
                response.Result = new
                {
                    MorningCount = morningData,
                    EveningCount = eveningData,
                    Total = total
                };
                return response;
            }
        }

        [Authorize]
        [HttpGet]
        [Route("/phase-wise-data")]
        public async Task<Response> GetPhaseWiseData(string startDate, string endDate)
        {
            var response = new Response();
            DateTime startDateTimeWithTime = DateTime.ParseExact(startDate, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            DateTime endDateTimeWithTime = DateTime.ParseExact(endDate, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            List<PhaseGender> alluserattendenceData = _db.RegisteredAttendenceData.Join(
                                    _db.StudentMasterData,
                                    t1 => t1.StudentId,
                                    t2 => t2.StudentId,
                                    (t1, t2) => new { Table1 = t1, Table2 = t2 }
                                )
                             .Where(user =>
                             user.Table1.IsActive == IsActiveEnum.Active &&
                             user.Table2.IsActive == IsActiveEnum.Active &&
                             user.Table1.AttendenceDate >= startDateTimeWithTime && user.Table1.AttendenceDate
                              < endDateTimeWithTime).Select(user =>
                             new PhaseGender()
                             {
                                 Phase = user.Table2.Category.ToLower(),
                                 Gender = user.Table2.Gender,
                             }
                             ).ToList();
            if (!alluserattendenceData.Any())
            {
                response.IsSuccess = false;
                response.ResponseMessage = "No user found.";
                return response;
            }
            else
            {
                
                var allPhaseData = new List<PhaseData>();
                var result = alluserattendenceData
                    .GroupBy(e => new { e.Phase, e.Gender })
                  .Select(g
                    => new
                    {
                        Phase = g.Key.Phase,
                        Gender = g.Key.Gender,
                        Count = g.Count()
                    })
                .GroupBy(d => d.Phase)
                .Select(f => new
                {
                    Phase = f.Key,
                    Boys = f.FirstOrDefault(p => p.Gender == Gender.Male)?.Count ?? 0,
                    Girls = f.FirstOrDefault(p => p.Gender == Gender.Female)?.Count ?? 0,
                    Total = f.Sum(h => h.Count)
                });
                response.IsSuccess = true;
                response.ResponseMessage = "user Found";
                response.Result = result;
                
                return response;
            }
        }

        [Authorize]
        [HttpGet]
        [Route("/get-graph-data")]
        public async Task<Response> GetGraphData(string date)
        {
            var response = new Response();
            DateTime dateNow = DateTime.ParseExact(date, "MM/dd/yyyy", CultureInfo.InvariantCulture);
            DateTime dateNowOnly = dateNow.Date;
            DateTime sevenDaysBackDateOnly = dateNow.Date.AddDays(-7);
            var alluserattendenceData = _db.RegisteredAttendenceData.Join(
                                    _db.StudentMasterData,
                                    t1 => t1.StudentId,
                                    t2 => t2.StudentId,
                                    (t1, t2) => new { Table1 = t1, Table2 = t2 }
                                )
                             .Where(user =>
                             user.Table1.IsActive == IsActiveEnum.Active &&
                             user.Table2.IsActive == IsActiveEnum.Active &&
                             user.Table1.AttendenceDate.Date >= sevenDaysBackDateOnly &&
                             user.Table1.AttendenceDate.Date <= dateNowOnly
                             ).Select(user =>
                             new ExcelAllResponseModel()
                             {
                                 StudentId = user.Table1.StudentId,
                                 StudentName = user.Table2.StudentName,
                                 StudentUniqueNumber = user.Table2.UniqueId,
                                 AttendenceDate = user.Table1.AttendenceDate,
                                 CreatedAt = user.Table1.CreatedAt,
                                 CreatedBy = user.Table1.CreatedBy,
                                 DayStatus = user.Table1.DayStatus
                             }
                             ).ToList();
            if (!alluserattendenceData.Any())
            {
                response.IsSuccess = false;
                response.ResponseMessage = "No user found.";
                return response;
            }
            else
            {

                //var result = alluserattendenceData
                //             .GroupBy(record => record.AttendenceDate.Date)
                //             .Select(group =>
                //                             new 
                //                             {
                //                                 Date = group.Key,
                //                                 MorningCount = group.Count(record => record.DayStatus == DayStatusEnum.Morning),
                //                                 EveningCount = group.Count(record => record.DayStatus == DayStatusEnum.Evening),
                //                                 TotalCount = group.Count(record => record.DayStatus == DayStatusEnum.Morning) + 
                //                                 group.Count(record => record.DayStatus == DayStatusEnum.Evening)
                //                             }
                //                         )
                //                         .ToList();

                List<DateTime> lastSevenDays = Enumerable.Range(0, 7)
                                        .Select(i => sevenDaysBackDateOnly.AddDays(i))
                                        .ToList();

                var result = lastSevenDays
                    .GroupJoin(alluserattendenceData,
                        date => date,
                        record => record.AttendenceDate.Date,
                        (date, records) =>
                            new
                            {
                                Date = date,
                                MorningCount = records.Count(record => record.DayStatus == DayStatusEnum.Morning),
                                 EveningCount = records.Count(record => record.DayStatus == DayStatusEnum.Evening),
                                TotalCount = records.Count(),
                            }
                    )
                    .ToList();

                // For dates with no data, add them with counts set to 0
                foreach (var data in lastSevenDays.Except(result.Select(item => item.Date)))
                {
                    result.Add(new
                    {
                        Date = data,
                        MorningCount = 0,
                        EveningCount = 0,
                        TotalCount = 0
                    });
                }

                // Sort the result by date
                result = result.OrderBy(item => item.Date).ToList();
                int morningData = 0;
                int eveningData = 0;
                foreach (var user in alluserattendenceData)
                {

                    if (user.DayStatus == DayStatusEnum.Morning)
                    {
                        morningData = morningData + 1;
                    }
                    else
                    {
                        eveningData = eveningData + 1;
                    }
                }
                int total = morningData + eveningData;

                response.IsSuccess = true;
                response.ResponseMessage = "user Found";
                response.Result = result;
                //response.Result = new
                //{
                //    MorningCount = morningData,
                //    EveningCount = eveningData,
                //    Total = total
                //};
                return response;
            }
        }


        [HttpPost]
        [Route("/authenticate")]
        public async Task<ActionResult<Response>> CreateToken([FromBody] Token createToken)
        {
            var response = new Response();
            try
            {
                var authenticateUser = await AuthenticateUser(createToken);
                if (!authenticateUser.IsSuccess)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = authenticateUser.ResponseMessage;
                }
                else
                {
                    var serializedData = JsonConvert.SerializeObject(authenticateUser.Result);
                    var authDatas = JsonConvert.DeserializeObject<AdminUser>(serializedData);
                    var authData = new AuthData
                    {
                        UserId = Convert.ToInt32(authDatas.AdminUserId),
                        UserName = authDatas.AdminUserName,
                        Gender = authDatas.Gender.ToString(),
                        UserStatus = authDatas.Status.ToString()

                };
                    var generateToken = _jwtAuthentication.GenerateJsonToken(authData);
                    response.IsSuccess = true;
                    response.ResponseMessage = "user is valid";
                    response.Result = generateToken;
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("/post-feedback")]
        public async Task<ActionResult<Response>> PostFeedback([FromBody] string feedback)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
                
                var postFeedbackData = new Feedback();
                postFeedbackData.FeedbackDescription = feedback.Trim();
                postFeedbackData.CreatedAt = indiaTime;
                postFeedbackData.CreatedBy = userId;
                _db.FeedbacksData.Add(postFeedbackData);
                await _db.SaveChangesAsync();
                response.IsSuccess = true;
                response.ResponseMessage = "Feedback is added.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;

        }

        [Authorize]
        [HttpPost]
        [Route("/post-note")]
        public async Task<ActionResult<Response>> PostNote([FromBody] string note)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

                var postNoteData = new Note();
                postNoteData.NoteDescription = note.Trim();
                postNoteData.CreatedAt = indiaTime;
                postNoteData.CreatedBy = userId;
                postNoteData.IsActive = IsActiveEnum.Active;
                _db.NotesData.Add(postNoteData);
                await _db.SaveChangesAsync();
                response.IsSuccess = true;
                response.ResponseMessage = "Note is added.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;

        }

        [Authorize]
        [HttpPut]
        [Route("/edit-note")]
        public async Task<ActionResult<Response>> EditNote([FromBody] int editNoteId, string note)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

                var editNoteData = _db.NotesData.FirstOrDefault(note => note.NoteId == editNoteId && note.IsActive == IsActiveEnum.Active);
                if (editNoteData is null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "note Id is not avialable.";
                    return response;
                }
                editNoteData.NoteDescription = note.Trim();
                editNoteData.UpdatedAt = indiaTime;
                editNoteData.UpdatedBy = userId;
                editNoteData.IsActive = IsActiveEnum.Active;
                await _db.SaveChangesAsync();
                response.IsSuccess = true;
                response.ResponseMessage = "edit Note is added.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;

        }

        [Authorize]
        [HttpPut]
        [Route("/delete-note")]
        public async Task<ActionResult<Response>> DeleteNote([FromBody] int noteId)
        {
            var response = new Response();
            int userId = 0;
            string userStatus = string.Empty;
            Response allClaims = await GetAuthDetails(User.Claims.ToList());
            if (allClaims.IsSuccess && allClaims.Result is not null)
            {
                var serializedData = JsonConvert.SerializeObject(allClaims.Result);
                var authData = JsonConvert.DeserializeObject<AuthData>(serializedData);
                userStatus = authData.UserStatus;
                userId = authData.UserId;
            }
            try
            {
                TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime indiaTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);

                var deleteNoteData = _db.NotesData.FirstOrDefault(note => note.NoteId == noteId && note.IsActive == IsActiveEnum.Active);
                if (deleteNoteData is null)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "note Id is not avialable.";
                    return response;
                }
                deleteNoteData.IsActive = IsActiveEnum.Inactive;
                deleteNoteData.UpdatedAt = indiaTime;
                deleteNoteData.UpdatedBy = userId;
                await _db.SaveChangesAsync();
                response.IsSuccess = true;
                response.ResponseMessage = "Note is deleted.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "there is an error.";
                response.Result = ex;
            }
            return response;

        }

        [Authorize]
        [HttpGet]
        [Route("/get-all-notes")]
        public async Task<IActionResult> GetAllNotes()
        {
            return Ok(_db.NotesData.Where(note => note.IsActive == IsActiveEnum.Active));
        }

        [Authorize]
        [HttpGet]
        [Route("/get-single-note")]
        public async Task<IActionResult> GetSingleNote(int noteId)
        {
            return Ok(_db.NotesData.Where(note => note.NoteId == noteId && note.IsActive == IsActiveEnum.Active));
        }

        private async Task<Response> AuthenticateUser(Token authenticateUser)
        {
            var response = new Response();

            var checkIfValidEmail = await _db.AdminUserData.Where(x => x.EmailId == authenticateUser.UserMailId && x.IsActive == IsActiveEnum.Active).FirstOrDefaultAsync();
            if(checkIfValidEmail is null)
            {
                response.IsSuccess = false;
                response.ResponseMessage = "No such user found";
                response.Result = NotFound();
            }
            else
            {
                var checkForValidPassword = BCrypt.Net.BCrypt.Verify(authenticateUser.Password, checkIfValidEmail.Password);
            if (!checkForValidPassword)
                {
                    response.IsSuccess = false;
                    response.ResponseMessage = "Password is not valid.";
                    response.Result = Unauthorized();
                }
                else
                {
                    response.IsSuccess = true;
                    response.ResponseMessage = "Password is valid.";
                    response.Result = checkIfValidEmail;
                }
                
            }
            return response;
        }

        private async Task<Response> GetAuthDetails(IEnumerable<Claim> userClaims)
        {
            var userDetails = new AuthData();
            var response = new Response();
            var userIdClaim = userClaims.FirstOrDefault(c => c.Type == "UserId");
            var userNameClaim = userClaims.FirstOrDefault(c => c.Type == "UserName");
            var userStatusClaim = userClaims.FirstOrDefault(c => c.Type == "UserStatus");

            if (userIdClaim != null && userNameClaim != null && userStatusClaim != null)
            {
                userDetails.UserId = Convert.ToInt32(userIdClaim.Value);
                userDetails.UserName = userNameClaim.Value;
                userDetails.UserStatus = userStatusClaim.Value;
                response.IsSuccess = true;
                response.ResponseMessage = "user token details fetched";
                response.Result = userDetails;
                return response;
            }
            else
            {
                response.IsSuccess = true;
                response.ResponseMessage = "user token details not fetched";
                response.Result = null;
                return response;
            }
        }
    }
}
