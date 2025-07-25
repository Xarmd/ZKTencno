namespace ZKTencno.Models
{
    public class AttendanceLog
    {
        public string EmployeeId { get; set; } = string.Empty;
        public DateTime LogTime { get; set; }
    }

    public class PunchLog
    {
        public string UserId { get; set; }
        public string Type { get; set; }  // Optional: CheckIn/Out
        public DateTime LogTime { get; set; }
    }

    public class RealTimePayload
    {
        public string OperationID { get; set; }
        public PunchLog PunchLog { get; set; }
        public string AuthToken { get; set; }
        public DateTime Time { get; set; }
    }

    public class RootPayload
    {
        public RealTimePayload RealTime { get; set; }
    }

}
