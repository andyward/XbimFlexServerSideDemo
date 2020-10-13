using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Xbim.Flex.DemoWebApp.Helpers
{
    public static class Config
    {
        // This should be in Config so they can be kept secure & configured at runtime

        public const string FlexApiBase = "https://api.xbim-dev.net";
        public const string FlexIdServerAddress = "https://id.xbim-dev.net";
        public const string ClientId = "BBDC34EE-F60C-42D4-B783-5F6279C9A846";
        // Xbim Flex will issue you with your client/secret
        public const string ClientSecret = "YourSecretThatYouNeedToKeepVerySafe"; 
    }
}