using ApiWithAuth.Resources;
using System.ComponentModel.DataAnnotations;

namespace ApiWithAuth.Models;

public class RegistrationRequest
{
    [Display(ResourceType = typeof(DisplayNameResource), Name = "Email")]
    [Required(ErrorMessageResourceType = typeof(ErrorMessageResource), ErrorMessageResourceName = "RequiredError")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ErrorMessageResource), ErrorMessageResourceName = "RequiredError")]
    [Display(ResourceType = typeof(DisplayNameResource), Name = "UserName")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessageResourceType = typeof(ErrorMessageResource), ErrorMessageResourceName = "RequiredError")]
    [Display(ResourceType = typeof(DisplayNameResource), Name = "Password")]
    public string Password { get; set; } = null!;
}
