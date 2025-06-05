namespace BasicFaceitServer.Events.DamageManagement;

public interface IDamageManagementApi
{
    delegate bool CallOriginalOnTakeDamageMethod();
    void Hook_OnTakeDamage(CallOriginalOnTakeDamageMethod handler);
    void Unhook_OnTakeDamage(CallOriginalOnTakeDamageMethod handler);
}