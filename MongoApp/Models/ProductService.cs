using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace MongoApp.Models
{
    public class ProductService
    {
        IGridFSBucket gridFS; // Файловое хранилище.
        IMongoCollection<Product> Products; // Колекция в базе данных.
        public ProductService()
        {
            // Строка подключения
            string connectionString = "mongodb://localhost:27017/mobilestore";
            var connection = new MongoUrlBuilder(connectionString);
            // получаем клиента для взаимодействия с базой данных
            MongoClient client = new MongoClient(connectionString);
            // получаем доступ к самой базе данных
            IMongoDatabase database = client.GetDatabase(connection.DatabaseName);
            // получаем доступ к файловому хранилищу
            gridFS = new GridFSBucket(database);
            // обращаемся к коллекции Products
            Products = database.GetCollection<Product>("Products");
        }
        // получаем все документы, используя критерии фильтрации
        public async Task< IEnumerable<Product>> GetProducts(int? minPrice, int? maxPrice,string name)
        {
            // строитель фильтров
            var builder = new FilterDefinitionBuilder<Product>();
            var filter = builder.Empty; // фильтр для выборки всех документов.
            
            if (!String.IsNullOrWhiteSpace(name))  // фильтр по имени
            {
                filter = filter & builder.Regex("Name", new BsonRegularExpression(name));
            }
            if (minPrice.HasValue) // фильтр по минимальной цене
            {
                filter = filter & builder.Gte("Price", minPrice.Value);
            }
            if(maxPrice.HasValue)  // фильтр по максимальной цене
            {
                filter = filter & builder.Lte("Price", maxPrice.Value);
            }
            return await Products.Find(filter).ToListAsync();
        }
        // Получаем один документ по id
        public async Task<Product> GetProduct(string id)
        {
            return await Products.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }
        // Добавление документа
        public async Task Create(Product p)
        {
            await Products.InsertOneAsync(p);
        }
        // Обновление документа
        public async Task Update(Product p)
        {
            await Products.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(p.Id)), p);
        }
        // Удаление документа
        public async Task Remove(string id)
        {
            await Products.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
        }
        // Получение изображения
        public async Task<byte[]> GetImage(string id)
        {
            return await gridFS.DownloadAsBytesAsync(new ObjectId(id));
        }
        // Сохранение изображения
        public async Task StoreImage(string id, Stream imageStream, string imageName)
        {
            Product p = await GetProduct(id);
            if (p.HasImage())
            {
                // Если ранее уже была прикреплена картинка, удаляем ее.
                gridFS.DeleteAsync(new ObjectId(p.ImageId));
            }
            // Сохраняем изображение
            ObjectId imageId = await gridFS.UploadFromStreamAsync(imageName, imageStream);
            // Обновляем данные по документу
            p.ImageId = imageId.ToString();
            var filter = Builders<Product>.Filter.Eq("_id", new ObjectId(p.Id));
            var update = Builders<Product>.Update.Set("ImageId", p.ImageId);
            await Products.UpdateOneAsync(filter, update);
        }
    }
}
