using static BasicFaceitServer.Events.DamageManagement.IDamageManagementApi;

namespace BasicFaceitServer.Events.DamageManagement
{
    public class DamageManagementApi : IDamageManagementApi
    {
        private readonly List<CallOriginalOnTakeDamageMethod> _handler = [];
        public void Hook_OnTakeDamage(CallOriginalOnTakeDamageMethod handler)
        {
            _handler.Add(handler);
        }

        public void Unhook_OnTakeDamage(CallOriginalOnTakeDamageMethod handler)
        {
            _handler.Remove(handler);
        }
        public bool IsNeedCallOriginalMethod()
        {
            return _handler.Select(handler => handler.Invoke()).FirstOrDefault();
        }
    }
}