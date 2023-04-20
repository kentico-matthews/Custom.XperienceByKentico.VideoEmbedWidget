using DancingGoat.FormComponents;
using Kentico.Xperience.Admin.Base.Forms;

[assembly: RegisterFormComponent(InvisibleFormComponent.IDENTIFIER,typeof(InvisibleFormComponent), InvisibleFormComponent.NAME)]

namespace DancingGoat.FormComponents
{
    [ComponentAttribute(typeof(InvisibleComponentAttribute))]
    public class InvisibleFormComponent : FormComponent<InvisibleProperties,InvisibleClientProperties,bool>
    {
        public const string IDENTIFIER = "Custom.InvisibleComponent";
        public const string NAME = "InvisibleComponent";

        public override string ClientComponentName => "@dancinggoat/web-admin/Invisible";
    }
}
