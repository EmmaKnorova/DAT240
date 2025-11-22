using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TarlBreuJacoBaraKnor.webapp.Core.Domain.Products;
using TarlBreuJacoBaraKnor.webapp.Infrastructure.Data;

namespace TarlBreuJacoBaraKnor.Pages.Admin.Products;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ShopContext _db;
    private readonly IWebHostEnvironment _env;

    public EditModel(ShopContext db, IWebHostEnvironment env)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    [BindProperty]
    public IFormFile? ImageUpload { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

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

        public string? ExistingImagePath { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var foodItem = await _db.FoodItems.FindAsync(id);
        
        if (foodItem is null)
        {
            TempData["ErrorMessage"] = "Product not found";
            return RedirectToPage("Index");
        }

        Input = new InputModel
        {
            Id = foodItem.Id,
            Name = foodItem.Name,
            Description = foodItem.Description,
            Price = foodItem.Price,
            CookTime = foodItem.CookTime,
            ExistingImagePath = foodItem.ImagePath
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var foodItem = await _db.FoodItems.FindAsync(Input.Id);
        
        if (foodItem is null)
        {
            TempData["ErrorMessage"] = "Product not found";
            return RedirectToPage("Index");
        }

        // Update basic fields
        foodItem.Name = Input.Name;
        foodItem.Description = Input.Description;
        foodItem.Price = Input.Price;
        foodItem.CookTime = Input.CookTime;

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

                // Delete the old image if it exists
                if (!string.IsNullOrWhiteSpace(Input.ExistingImagePath))
                {
                    var oldPhysicalPath = Path.Combine(
                        _env.WebRootPath,
                        Input.ExistingImagePath.TrimStart('/', '\\')
                    );

                    if (System.IO.File.Exists(oldPhysicalPath))
                    {
                        System.IO.File.Delete(oldPhysicalPath);
                    }
                }

                foodItem.ImagePath = Path.Combine("images", "products", fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ImageUpload", $"Error uploading image: {ex.Message}");
                return Page();
            }
        }

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Product '{Input.Name}' updated successfully!";
        return RedirectToPage("Index");
    }
}