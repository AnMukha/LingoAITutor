using LingoAITutor.Host.Entities;
using System.Security.Claims;

namespace LingoAITutor.Host.Endpoints
{
    public class UserIdHepler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserIdHepler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Guid GetUserId()
        {
            var cp = _httpContextAccessor.HttpContext!.User;
            var user = cp.FindFirst(claim => claim.Type == "id");
            return Guid.Parse(user.Value);
        }
    }
}
