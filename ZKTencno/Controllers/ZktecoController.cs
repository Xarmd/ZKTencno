using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZKTencno.Models;

namespace ZKTencno.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZktecoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ZktecoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("save-logs")]
        public IActionResult SaveLogs([FromQuery] string? ip = null, [FromQuery] int port = 4370)
        {
            ip ??= "192.168.1.201";

            var helper = new ZktecoHelper();

            if (!helper.Connect(ip, port))
            {
                return BadRequest("Could not connect to device.");
            }

            // This must return List<AttendanceLog>
            List<AttendanceLog> logs = helper.GetStructuredLogs(simulate: true);

            helper.Disconnect();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (var log in logs)
                {
                    using (SqlCommand cmd = new SqlCommand(
                        @"INSERT INTO AttendanceLogs (EmployeeId, LogTime) 
                          VALUES (@EmployeeId, @LogTime)", conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", log.EmployeeId);
                        cmd.Parameters.AddWithValue("@LogTime", log.LogTime);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return Ok(new { success = true, inserted = logs.Count });
        }

        [HttpPost("save-logs")]
        public IActionResult SaveLogs([FromBody] List<AttendanceLog> logs)
        {
            if (logs == null || logs.Count == 0)
                return BadRequest("No logs received.");

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (var log in logs)
                {
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO AttendanceLogs (EmployeeId, LogTime) VALUES (@EmployeeId, @LogTime)", conn))
                    {
                        cmd.Parameters.AddWithValue("@EmployeeId", log.EmployeeId);
                        cmd.Parameters.AddWithValue("@LogTime", log.LogTime);
                        cmd.ExecuteNonQuery();
                    }
                }

                conn.Close();
            }

            return Ok(new { success = true, inserted = logs.Count });
        }

        [HttpPost("realtime-callback")]
        public IActionResult SavePayload([FromBody] object payload)
        {
            string jsonString = payload.ToString(); // Converts any incoming payload into JSON string

            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO TempLogs (Name, CreatedOn) VALUES (@Name, @CreatedOn)", conn);
                cmd.Parameters.AddWithValue("@Name", jsonString);
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.Now);
                cmd.ExecuteNonQuery();
            }

            var payloadDes = JsonSerializer.Deserialize<RootPayload>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payloadDes?.RealTime?.PunchLog == null)
                return BadRequest("Invalid data");

            var log = payloadDes.RealTime.PunchLog;

            // Save to DB
            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO AttendanceLogs (EmployeeId, LogTime) VALUES (@EmployeeId, @LogTime)", conn);
                cmd.Parameters.AddWithValue("@EmployeeId", log.UserId);
                cmd.Parameters.AddWithValue("@LogTime", log.LogTime);
                cmd.ExecuteNonQuery();
            }

            return Ok(new { success = true });
        }
    }
}
