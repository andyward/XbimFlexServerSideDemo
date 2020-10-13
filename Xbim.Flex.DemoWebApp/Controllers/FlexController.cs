using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Xbim.Flex.Client;
using Xbim.Flex.DemoWebApp.Models;

namespace Xbim.Flex.DemoWebApp.Controllers
{
    public class FlexController : Controller
    {
        private const string FlexApiBase = "https://api.xbim-dev.net";
        private const string FlexIdServerAddress = "https://id.xbim-dev.net";

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
            var client = BuildApiClient(token);
            var tenants = await GetTenants(client);

            ViewBag.AccessToken = token.AccessToken;

            return View(tenants);
        }

        public async Task<ActionResult> Models()
        {

            var token = await GetAccessTokenAsync();
            var client = BuildApiClient(token);
            var models = await GetModels(client);

            return View(models);
        }

        // Opens a model
        public async Task<ActionResult> InspectModel(int id)
        {

            var token = await GetAccessTokenAsync();
            var client = BuildApiClient(token);
            var rooms = await GetSpaces(client, $"AssetModelId eq {id}");
            var tenantId = (await GetCurrentTenant(client)).Identifier;
            var model = await client.Models_GetAsync(id, tenantId);

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
            var client = BuildApiClient(token);

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
            var client = BuildApiClient(token);

            var assetName = "Dorm";
            
            var tenantId = (await GetCurrentTenant(client)).Identifier;
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
            var client = BuildApiClient(token);
            var tenantId = (await GetCurrentTenant(client)).Identifier;
            var model = await client.Models_GetAsync(id, tenantId);

            var file = await GetWexbimEnvelope(client, model);

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

        async Task<Tenant> GetCurrentTenant(FlexAPI client)
        {
            var tenants = await GetTenants(client);

            // TODO: Get from state. For this simple demo we take the first one
            var result = tenants.FirstOrDefault() ?? throw new Exception("No tenant");
            return result;
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
            if(Session["Token"] is TokenResponse tr)
            {
                return tr;
            }

            var client = new HttpClient();

            // discover the OAuh2 endpoints.
            var disco = await client.GetDiscoveryDocumentAsync(FlexIdServerAddress);
            if (disco.IsError) throw new Exception(disco.Error);

            // Use Client Credentials flow. Note this means it's *this app* that is authenticated, not an *end user*.
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                Scope = "api.write",
                ClientId = "BBDC34EE-F60C-42D4-B783-5F6279C9A846",
                ClientSecret = "YourSecretThatYouNeedToKeepVerySafe",   // We will issue you with your client/secret
            });

            if (response.IsError) throw new Exception(response.Error);

            // We're storing state on the session - you may have a better means of caching state
            Session["Token"] = response;
            return response;
        }

        async Task<IEnumerable<Tenant>> GetTenants(FlexAPI flexClient)
        {
            if (Session["Tenants"] is IEnumerable<Tenant> tenants)
            {
                return tenants;
            }

            var response = await flexClient.Tenants_GetAsync();

            Session["Tenants"] = response.Value;
            return response.Value;
        }

        async Task<IEnumerable<Model>> GetModels(FlexAPI flexClient)
        {
            var tenant = await GetCurrentTenant(flexClient);
            var response = await flexClient.Models_GetAsync(tenant.Identifier /* filters, sorts, paging */ );

            return response.Value;
        }

        async Task<IEnumerable<Space>> GetSpaces(FlexAPI flexClient, string filter /*, Criteria */)
        {
            var tenant = await GetCurrentTenant(flexClient);
            var response = await flexClient.Spaces_GetAsync(tenant.Identifier, filter: filter, orderby: "Name", expand: "Components" /* filters, paging */ );

            return response.Value;
        }

        async Task<FileResponse> GetWexbimEnvelope(FlexAPI flexClient, Model model)
        {
            var tenant = await GetCurrentTenant(flexClient);
            var response = await flexClient.WexbimEnvelope_GetAsync(model.AssetId.Value, model.AssetModelId.Value, tenant.Identifier);
            // And others
            //var response = await flexClient.WexbimComponents_GetAsync(model.AssetId.Value, model.AssetModelId.Value, tenant.Identifier);
            //var response = await flexClient.WexbimSpatial_GetAsync(model.AssetId.Value, model.AssetModelId.Value, tenant.Identifier);
            //var response = await flexClient.WexbimWindowsdoors_GetAsync(model.AssetId.Value, model.AssetModelId.Value, tenant.Identifier);

            return response;
        }

        private static FlexAPI BuildApiClient(TokenResponse token)
        {
            var httpClient = new HttpClient();

            httpClient.SetBearerToken(token.AccessToken);

            var flexClient = new FlexAPI(httpClient)
            {
                BaseUrl = FlexApiBase
            };
            return flexClient;
        }
    }
}