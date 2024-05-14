using Microsoft.AspNetCore.Mvc;
using MultipleImageHandling.Models;
using MultipleImageHandling.Services;
using System;

namespace MultipleImageHandling.Controllers
{
    public class ItemsController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment environment;

        public ItemsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            //var items=context.Items.ToList();
            var items = context.Items.OrderByDescending(i => i.Id).ToList();
            return View(items);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ItemDto itemDto)
        {
            if (itemDto.ImageFiles == null || !itemDto.ImageFiles.Any())
            {
                ModelState.AddModelError("ImageFiles", "At least one image is required");
            }
            if (!ModelState.IsValid)
            {
                return View(itemDto);
            }

            List<string> imageFileNames = new List<string>();

            foreach (var imageFile in itemDto.ImageFiles)
            {
                string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(imageFile.FileName);
                string imageFullPath = Path.Combine(environment.WebRootPath, "Items", newFileName);

                using (var stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                imageFileNames.Add(newFileName);
            }

            Item item = new Item()
            {
                Name = itemDto.Name,
                Unit = itemDto.Unit,
                Quantity = itemDto.Quantity,
                ImageFileNames = string.Join(",", imageFileNames),
                CreatedAt = DateTime.Now,
            };

            context.Items.Add(item);
            context.SaveChanges();

            return RedirectToAction("Index", "Items");
        }

        public IActionResult Edit(int id)
        {
            var item = context.Items.Find(id);
            if (item == null)
            {
                return RedirectToAction("Index", "Items");
            }

            // Create ItemDto from Item
            var itemDto = new ItemDto()
            {
                Name = item.Name,
                Unit = item.Unit,
                Quantity = item.Quantity,
            };

            ViewData["ItemID"] = item.Id;
            ViewData["ImageFileNames"] = item.ImageFileNames.Split(','); // Split the filenames to pass to the view
            ViewData["CreatedAt"] = item.CreatedAt.ToString("dd/MM/yyyy");

            return View(itemDto);
        }

        [HttpPost]
        public IActionResult Edit(int id, ItemDto itemDto)
        {
            var item = context.Items.Find(id);
            if (item == null)
            {
                return RedirectToAction("Index", "Items");
            }

            if (!ModelState.IsValid)
            {
                ViewData["ItemID"] = item.Id;
                ViewData["ImageFileNames"] = item.ImageFileNames.Split(','); // Pass existing image filenames to the view
                ViewData["CreatedAt"] = item.CreatedAt.ToString("dd/MM/yyyy");
                return View(itemDto);
            }

            List<string> newImageFileNames = new List<string>(); // List to store new image filenames

            if (itemDto.ImageFiles != null && itemDto.ImageFiles.Any())
            {
                // Delete old images first
                var oldImages = item.ImageFileNames.Split(',');
                foreach (var oldImage in oldImages)
                {
                    string oldImageFullPath = Path.Combine(environment.WebRootPath, "Items", oldImage);
                    if (System.IO.File.Exists(oldImageFullPath))
                    {
                        System.IO.File.Delete(oldImageFullPath);
                    }
                }

                // Upload and save new images
                foreach (var imageFile in itemDto.ImageFiles)
                {
                    // Generate a new filename for each new image
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(imageFile.FileName);
                    string imageFullPath = Path.Combine(environment.WebRootPath, "Items", newFileName);

                    using (var stream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    newImageFileNames.Add(newFileName);
                }
            }

            // Update the item in the database
            item.Name = itemDto.Name;
            item.Unit = itemDto.Unit;
            item.Quantity = itemDto.Quantity;
            item.ImageFileNames = string.Join(",", newImageFileNames); // Save new image filenames as comma-separated string

            context.SaveChanges();

            return RedirectToAction("Index", "Items");
        }

        public IActionResult Delete(int id)
        {
            var item = context.Items.Find(id);
            if (item == null)
            {
                return RedirectToAction("Index", "Items");
            }

            // Split the ImageFileNames string into an array of filenames
            var imageFileNames = item.ImageFileNames.Split(',');

            // Delete each image file
            foreach (var imageFileName in imageFileNames)
            {
                string imageFullPath = Path.Combine(environment.WebRootPath, "Items", imageFileName);
                if (System.IO.File.Exists(imageFullPath))
                {
                    System.IO.File.Delete(imageFullPath);
                }
            }

            context.Items.Remove(item);
            context.SaveChanges();

            return RedirectToAction("Index", "Items");
        }

    }
}
