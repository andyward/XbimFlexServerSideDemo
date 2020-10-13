using System.Collections.Generic;
using Xbim.Flex.Client;

namespace Xbim.Flex.DemoWebApp.Models
{

    // An MVC "ViewModel" for inspecting a Flex Model
    public class InspectModel
    {
        public Model FlexModel { get; set; }
        public IEnumerable<Space> Spaces { get; set; }
    }
}