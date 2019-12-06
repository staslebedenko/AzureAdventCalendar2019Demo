using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ServerlessSqlLoadTesting
{
    public class LoadTestController
    {
        private readonly ILogger log;

        private readonly DemoDbContext context;

        public LoadTestController(ILogger<LoadTestController> log, DemoDbContext context)
        {
            this.log = log;
            this.context = context;
        }

        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequest req)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];
            string groupName = req.Query["groupName"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;
            groupName = groupName ?? data?.groupName;

            await this.SavePerson(name, groupName);

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        [FunctionName("LoaderActivation")]
        public static async Task<IActionResult> LoaderActivation(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "loaderio-")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Loader.io validation triggered.");
            return (ActionResult)new OkObjectResult($"loaderio-");
        }


        private async Task SavePerson(string name, string groupName)
        {
            var group = new Group() {Name = groupName};
            var person = new Person() { Name = name, Group  = group};
            await this.context.AddAsync(person);
            await this.context.SaveChangesAsync();
        }
    }
}
