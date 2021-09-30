using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoApp.Models
{
    public class Product
    {
        //В MongoDB уникальный идентификатор документа имеет тип ObjectId, и по умолчанию
        //    для него в документе выделяется поле с именем "_id". И чтобы связать свойство
        //    Id класса Product с этим полем, над данным свойством установлен атрибут
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [Display(Name ="Модель")]
        public string Name { get; set; }

        [Display(Name ="Производитель")]
        public string Company { get; set; }
        [Display(Name ="Price")]
        public string Price { get; set; }

        public string ImageId { get; set; } // ссылка на изображение.

        public bool HasImage()
        {
            return !String.IsNullOrWhiteSpace(ImageId);
        }


    }
}
