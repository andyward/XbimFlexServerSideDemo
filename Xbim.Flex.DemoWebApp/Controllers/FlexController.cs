using IdentityModel.Client;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using Xbim.Flex.Client;
using Xbim.Flex.DemoWebApp.Helpers;
using Xbim.Flex.DemoWebApp.Models;

namespace Xbim.Flex.DemoWebApp.Controllers
{
    public class FlexController : Controller
    {
        
        private const string DataHome = "~/App_Data/";
        // Hardwired to a test model deployed with the Source
        private const string IfcModel = "Dormitory-ARC.ifczip";

        // GET: Flex Home page
        public ActionResult Index()
        {
            return View();
        }


        public async Task<ActionResult> Tenants()
        {
            
            var token = await GetAccessTokenAsync();
            var client = OAuthHelper.BuildApiClient(token);
            var tenants = await GetTenants(client);
            var user = await client.GetMeAsync();

            ViewBag.AccessToken = token.AccessToken;
            ViewBag.User = user;

            return View(tenants);
        }

        public async Task<ActionResult> Models()
        {

            var token = await GetAccessTokenAsync();
            var client = OAuthHelper.BuildApiClient(token);
            var models = await client.GetModelsAsync();

            return View(models);
        }

        // Opens a model
        public async Task<ActionResult> InspectModel(int id)
        {

            var token = await GetAccessTokenAsync();
            var client = OAuthHelper.BuildApiClient(token);
            var rooms = await client.GetSpacesAsync($"AssetModelId eq {id}");
            var model = await client.GetModelByIdAsync(id);

            var vm = new InspectModel
            {
                FlexModel = model,
                Spaces = rooms,
            };
            return View(vm);
        }


        public async Task<ActionResult> CreateTenant()
        {
            var token = await GetAccessTokenAsync();
            var client = OAuthHelper.BuildApiClient(token);

            var tenant = new TenantCreate
            {
                // TODO take from form input
                Name = "Your Flex tenant"
            };
            await client.Tenants_PostAsync(tenant);
            // Clear cache
            Session["Tenants"] = null;
  
            return RedirectToAction("Tenants");
        }


        public async Task<ActionResult> UploadModel()
        {
            var token = await GetAccessTokenAsync();
            var client = OAuthHelper.BuildApiClient(token);

            var assetName = "Dorm";
            
            var tenantId = (await client.GetCurrentTenantAsync()).Identifier;
            var filename = Path.Combine(DataDir, IfcModel);
            using (var filestream = System.IO.File.OpenRead(filename))
            {
                var file = new FileParameter(filestream, Path.GetFileName(IfcModel));
                await client.ModelfilesUpload_PostAsync(file, assetName, tenantId);
            }
                        
            return RedirectToAction("Models");
        }

        // To be consumed by the Xbim Flex WebGL viewer
        // In essence we're acting as a proxy here.
        public async Task<ActionResult> DownloadEnvelopeWexbim(int id)
        {
            var token = await GetAccessTokenAsync();
            var client = OAuthHelper.BuildApiClient(token);
            var tenantId = (await client.GetCurrentTenantAsync()).Identifier;
            var model = await client.Models_GetAsync(id, tenantId);

            var file = await client.GetWexbimEnvelopeAsync(model);

            if(file.StatusCode < 300)
            {
                // TODO: You could cache this on the server as the geometry should not change
                var fileName = $"model-{id}-envelope.wexbim";
                return File(file.Stream, "application/octet-stream", fileName);
            }
            else
            {
                return this.Content($"Failed to download Wexbim: {file.StatusCode}");
            }
            
        }

        private string DataDir
        {
            get
            {
                return Server.MapPath(DataHome);
            }
        }

        /// <summary>
        /// Gets an Access Token from Flex ID server using ClientCredentials Flow
        /// </summary>
        /// <returns></returns>
        async Task<TokenResponse> GetAccessTokenAsync()
        {
            if (Session["Token"] is TokenResponse tr)
            {
                return tr;
            }

            TokenResponse response = await OAuthHelper.GetClientAccessToken();

            // We're storing state on the session - you may have a better means of caching state
            Session["Token"] = response;
            return response;
        }

        // provides a cache around the current tenants
        async Task<IEnumerable<Tenant>> GetTenants(FlexAPI flexClient)
        {
            if (Session["Tenants"] is IEnumerable<Tenant> tenants)
            {
                return tenants;
            }

            var response = await flexClient.GetTenantsAsync();

            Session["Tenants"] = response;
            return response;
        }
        
    }
}