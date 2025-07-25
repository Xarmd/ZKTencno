using zkemkeeper;
using ZKTencno.Models;

public class ZktecoHelper
{
    private CZKEM device = new CZKEM();

    public bool Connect(string ip, int port)
    {
        return device.Connect_Net(ip, port);
    }

    public void Disconnect()
    {
        device.Disconnect();
    }

    public List<AttendanceLog> GetStructuredLogs(bool simulate = false)
    {
        var logs = new List<AttendanceLog>();
        int workcode = 0; // Must initialize before passing as ref

        if (simulate)
        {
            logs.Add(new AttendanceLog { EmployeeId = "1001", LogTime = DateTime.Now.AddMinutes(-10) });
            logs.Add(new AttendanceLog { EmployeeId = "1002", LogTime  = DateTime.Now.AddMinutes(-5) });
            return logs;
        }

        device.ReadGeneralLogData(1);

        while (device.SSR_GetGeneralLogData(1, out string enrollNumber, out int verifyMode, out int inOutMode,
        out int year, out int month, out int day, out int hour, out int minute, out int second, ref workcode))
            {
            var dt = new DateTime(year, month, day, hour, minute, second);
            logs.Add(new AttendanceLog { EmployeeId = enrollNumber, LogTime = dt });
        }

        return logs;
    }

}
