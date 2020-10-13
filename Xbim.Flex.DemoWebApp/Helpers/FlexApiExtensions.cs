using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Xbim.Flex.Client;

namespace Xbim.Flex.DemoWebApp.Helpers
{
    public static class FlexApiExtensions
    {
        public static async Task<IEnumerable<Model>> GetModelsAsync(this FlexAPI flexClient)
        {
            var tenant = await GetCurrentTenantAsync(flexClient);
            var response = await flexClient.Models_GetAsync(tenant.Identifier /* filters, sorts, paging */ );

            return response.Value;
        }

        public static async Task<Model> GetModelByIdAsync(this FlexAPI flexClient, int modelId)
        {
            var tenant = await GetCurrentTenantAsync(flexClient);
            return await flexClient.Models_GetAsync(modelId, tenant.Identifier);
        }

        public static async Task<IEnumerable<Tenant>> GetTenantsAsync(this FlexAPI flexClient)
        {
            
            var response = await flexClient.Tenants_GetAsync();

            return response.Value;
        }

        public static async Task<IEnumerable<Space>> GetSpacesAsync(this FlexAPI flexClient, string filter /*, Criteria */)
        {
            var tenant = await GetCurrentTenantAsync(flexClient);
            var response = await flexClient.Spaces_GetAsync(tenant.Identifier, filter: filter, orderby: "Name", expand: "Components" /* filters, paging */ );

            return response.Value;
        }

        public static async Task<Tenant> GetCurrentTenantAsync(this FlexAPI client)
        {
            var tenants = await GetTenantsAsync(client);

            // TODO: Get from state. For this simple demo we take the first one
            var result = tenants.FirstOrDefault() ?? throw new Exception("No tenant");
            return result;
        }

        public static async Task<FileResponse> GetWexbimEnvelopeAsync(this FlexAPI flexClient, Model model)
        {
            var tenant = await GetCurrentTenantAsync(flexClient);
            var response = await flexClient.WexbimEnvelope_GetAsync(model.AssetId.Value, model.AssetModelId.Value, tenant.Identifier);
            // And others
            //var response = await flexClient.WexbimComponents_GetAsync(model.AssetId.Value, model.AssetModelId.Value, tenant.Identifier);
            //var response = await flexClient.WexbimSpatial_GetAsync(model.AssetId.Value, model.AssetModelId.Value, tenant.Identifier);
            //var response = await flexClient.WexbimWindowsdoors_GetAsync(model.AssetId.Value, model.AssetModelId.Value, tenant.Identifier);

            return response;
        }

    }
}