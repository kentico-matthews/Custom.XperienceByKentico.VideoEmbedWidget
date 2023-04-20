using Kentico.Forms.Web.Mvc;
using DancingGoat.FormComponents;

[assembly: RegisterFormComponent(InvisibleFormComponent.IDENTIFIER, typeof(InvisibleFormComponent), InvisibleFormComponent.NAME, ViewName = "~/Components/FormComponents/InvisibleComponent/_Invisible.cshtml", IsAvailableInFormBuilderEditor = false)]
namespace DancingGoat.FormComponents
{
    public class InvisibleFormComponent : FormComponent<InvisibleProperties,bool>
    {
        public const string IDENTIFIER = "Custom.InvisibleComponent";
        public const string NAME = "InvisibleComponent";


        public bool Value { get; set; }


        public override bool GetValue()
        {
            return Value;
        }


        public override void SetValue(bool value)
        {
            Value = value;
        }
    }
}
