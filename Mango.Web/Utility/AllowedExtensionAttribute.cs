using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility
{
    public class AllowedExtensionAttribute : ValidationAttribute
    {
        private readonly string[] extensions;

        public AllowedExtensionAttribute(string[] extensions)
        {
            this.extensions = extensions;
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);   
                if(extensions.Contains(extension.ToLower())) 
                {
                    return new ValidationResult("This Photo extension is not allowed!");
                }
            }

            return ValidationResult.Success;
        }
    }
}
