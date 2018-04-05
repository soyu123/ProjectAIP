using DataAccessLayer.Core.Interfaces.UoW;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ProjectAIP.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IUnitOfWork uow, ILoggerFactory loggerFactory) : base(uow, loggerFactory)
        {
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }
    }
}
