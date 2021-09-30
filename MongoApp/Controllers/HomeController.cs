using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MongoApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService db;

        //Поскольку ProductService внедряется в приложение в виде сервиса,
        //то в конструкторе контроллера мы можем его получить.
        public HomeController(ProductService context)
        {
            db = context;
        }

        //В методе Index получаем введенные пользователем данные в виде модели FilterViewModel
        //и передаем эти данные в вызов метода db.GetProducts, через который извлекаем
        //из базы данных все соответствующие критериям фильтрации документы.
        public async Task<IActionResult> Index(FilterViewModel filter)
        {
            var phones = await db.GetProducts(filter.MinPrice, filter.MaxPrice, filter.Name);
            var model = new IndexViewModel { Products = phones, Filter = filter };
            return View(model);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product p)
        {
            if (ModelState.IsValid)
            {
                await db.Create(p);
                return RedirectToAction("Index");
            }
            return View();
        }
        public async Task<IActionResult> Edit(string id)
        {
            Product p = await db.GetProduct(id);
            if (p == null)
                return NotFound();
            return View(p);
        }
        //Действие Edit() изменяет документ в базе данных через вызов метода
        //Update класса ProductService.
        [HttpPost]
        public async Task<IActionResult> Edit(Product p)
        {
            if (ModelState.IsValid)
            {
                await db.Update(p);
                return RedirectToAction("Index");
            }
            return View(p);
        }
        //Действие Delete() удаляет документ по id, вызывая метод Remove кдасса ProductService.
        public async Task<IActionResult> Delete(string id)
        {
            await db.Remove(id);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> AttachImage(string id)
        {
            Product p = await db.GetProduct(id);
            if (p == null)
                return NotFound();
            return View(p);
        }
        //Действие AttachImage() получает отправленный пользователем файл и передает файловый поток
        //в метод StoreImage класса ProductService.
        [HttpPost]
        public async Task<ActionResult> AttachImage(string id, IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                await db.StoreImage(id, uploadedFile.OpenReadStream(), uploadedFile.FileName);
            }
            return RedirectToAction("Index");
        }
        //И действие GetImage() просто возвращает изображение по id.
        public async Task<ActionResult> GetImage(string id)
        {
            var image = await db.GetImage(id);
            if (image == null)
            {
                return NotFound();
            }
            return File(image, "image/png");
        }
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}

        //public IActionResult Index()
        //{
        //    return View();
        //}

        //public IActionResult Privacy()
        //{
        //    return View();
        //}

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
