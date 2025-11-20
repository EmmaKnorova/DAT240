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

        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2048)]
        public string Description { get; set; } = string.Empty;

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        public int CookTime { get; set; }

        public string? ExistingImagePath { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var foodItem = await _db.FoodItems.FindAsync(id);
        if (foodItem is null)
            return NotFound();

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
            return Page();

        var foodItem = await _db.FoodItems.FindAsync(Input.Id);
        if (foodItem is null)
            return NotFound();

        // Update basic fields
        foodItem.Name = Input.Name;
        foodItem.Description = Input.Description;
        foodItem.Price = Input.Price;
        foodItem.CookTime = Input.CookTime;

        // Handle image upload
        if (ImageUpload is not null && ImageUpload.Length > 0)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(ImageUpload.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ImageUpload.CopyToAsync(stream);
            }

            // Optionally delete the old image if it exists
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
            //foodItem.ImagePath = $"/images/products/{fileName}";
        }

        await _db.SaveChangesAsync();

        return RedirectToPage("Index");
    }
}