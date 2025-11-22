using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products.Pipelines;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Products;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly IMediator _mediator;
    private readonly IWebHostEnvironment _env;

    public CreateModel(IMediator mediator, IWebHostEnvironment env)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    [BindProperty]
    public IFormFile? ImageUpload { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 120 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2048, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2048 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100,000")]
        [Display(Name = "Price (NOK)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Cook time is required")]
        [Range(0, 1440, ErrorMessage = "Cook time must be between 0 and 1440 minutes")]
        [Display(Name = "Cook Time (minutes)")]
        public int CookTime { get; set; }
    }

    public void OnGet()
    {
        // Initialize with default values if needed
        Input = new InputModel
        {
            Price = 0,
            CookTime = 15
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string? imagePath = null;

        // Handle image upload
        if (ImageUpload is not null && ImageUpload.Length > 0)
        {
            // Validate file size (5MB max)
            if (ImageUpload.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ImageUpload", "Image file size cannot exceed 5MB");
                return Page();
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(ImageUpload.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ImageUpload", "Only image files (JPG, PNG, GIF, WebP) are allowed");
                return Page();
            }

            try
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageUpload.CopyToAsync(stream);
                }

                imagePath = Path.Combine("images", "products", fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ImageUpload", $"Error uploading image: {ex.Message}");
                return Page();
            }
        }

        // Create the product using MediatR pipeline
        var request = new Create.Request(
            Input.Name,
            Input.Description,
            Input.Price,
            Input.CookTime,
            imagePath
        );

        var response = await _mediator.Send(request);

        if (!response.Success)
        {
            foreach (var error in response.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return Page();
        }

        TempData["SuccessMessage"] = $"Product '{Input.Name}' created successfully!";
        return RedirectToPage("Index");
    }
}