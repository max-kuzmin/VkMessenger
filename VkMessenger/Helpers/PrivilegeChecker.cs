using Tizen.Security;

namespace ru.MaxKuzmin.VkMessenger.Helpers
{
    public static class PrivilegeChecker
    {
        public static void PrivilegeCheck(string permission)
        {
            var result = PrivacyPrivilegeManager.CheckPermission(permission);

            switch (result)
            {
                case CheckResult.Allow:
                    break;
                case CheckResult.Deny:
                case CheckResult.Ask:
                    PrivacyPrivilegeManager.RequestPermission(permission);
                    break;
            }
        }
    }
}
