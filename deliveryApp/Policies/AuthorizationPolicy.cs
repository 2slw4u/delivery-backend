using Microsoft.AspNetCore.Authorization;

namespace deliveryApp.Policies
{
    public class AuthorizationPolicy : IAuthorizationRequirement
    {
        public AuthorizationPolicy() {
        }
    }
}
