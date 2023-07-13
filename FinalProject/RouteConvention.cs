using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace FinalProject
{
    public class RouteConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if(controller.ControllerType.IsGenericType)
            {
                controller.ControllerName = controller.ControllerType.GenericTypeArguments[0].Name;
            }
        }
    }
}
