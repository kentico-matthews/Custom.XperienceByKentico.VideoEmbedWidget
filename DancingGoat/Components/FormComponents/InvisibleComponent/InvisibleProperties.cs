using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;

namespace DancingGoat.FormComponents
{
    public class InvisibleProperties : FormComponentProperties<bool>
    {
        public InvisibleProperties() : base(FieldDataType.Boolean)
        {
        }


        public override bool DefaultValue { get; set; }


        public override string Label { get => string.Empty; set => base.Label = value; }
    }
}
