using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ElevatedTutors.Models;


namespace ElevatedTutors.Filters
{
    public class ApprovedUserHandler : AuthorizationHandler<ApprovedUserRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        
        public ApprovedUserHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ApprovedUserRequirement requirement)
        {
            var user = await _userManager.GetUserAsync(context.User);
            if (user != null && user.IsApproved)
            {
                context.Succeed(requirement);
            }
        }
    }
}
