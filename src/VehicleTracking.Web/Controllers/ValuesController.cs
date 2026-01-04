using System.Web.Http;

namespace VehicleTracking.Web.Controllers
{
    public class ValuesController : ApiController
    {
        public IHttpActionResult Get()
        {
            return Ok(new string[] { "value1", "value2" });
        }
    }
}