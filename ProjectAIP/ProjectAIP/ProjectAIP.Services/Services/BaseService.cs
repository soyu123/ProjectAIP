using DataAccessLayer.Core.Interfaces.UoW;
using Microsoft.Extensions.Logging;

namespace ProjectAIP.Services.Services
{
    public class BaseService
    {
        protected readonly IUnitOfWork UoW;
        protected readonly ILogger Logger;
        public BaseService(IUnitOfWork uow, ILoggerFactory loggerFactory)
        {
            UoW = uow;
            Logger = loggerFactory.CreateLogger<BaseService>();
        }
    }
}
