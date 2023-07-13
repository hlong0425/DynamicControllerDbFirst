using Domain;
using FinalProject.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace FinalProject
{
    public class GenericTypeControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {   
        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            var types = DomainsBuilder.Assembly.GetTypes().ToList();

            foreach(var type in types)
            {
                feature.Controllers.Add(typeof(BaseController<>).MakeGenericType(type).GetTypeInfo());
            }
        }
    }
}