using System.Web;
using System.Web.Security;
using Newtonsoft.Json;

namespace MvcSiteMapProviderDemo.ViewModels
{
    public class UserInfoViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string[] Roles { get; set; }

        public static UserInfoViewModel GetCurrent()
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
                return null;

            // 先取得該使用者的 FormsIdentity
            FormsIdentity id = (FormsIdentity)HttpContext.Current.User.Identity;
            // 再取出使用者的 FormsAuthenticationTicket
            FormsAuthenticationTicket ticket = id.Ticket;

            return JsonConvert.DeserializeObject<UserInfoViewModel>(ticket.UserData);
        }
    }
}