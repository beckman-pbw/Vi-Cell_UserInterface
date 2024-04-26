using ScoutUtilities.Enums;

namespace HawkeyeCoreAPI.Interfaces
{
	public interface IAuditLog
	{
		void WriteToAuditLogAPI(string username, audit_event_type type, string resource);
	}
}
