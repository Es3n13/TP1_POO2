using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowerShop.Class;

namespace FlowerShop.Class
{
    public class UserJsonConverter : JsonConverter<Users>
    {
        public override Users ReadJson(JsonReader reader, Type objectType, Users existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            string name = jo["Name"].ToString();
            string email = jo["Email"].ToString();
            string id = jo["ID"].ToString();
            string role = jo["Role"].ToString();

            Users user = role switch
            {
                "Client" => new Client(name, email, id),
                "Seller" => new Seller(name, email, id),
                "Owner" => new Owner(name, email, id),
                "Supplier" => new Supplier(
                    name,
                    email,
                    jo["CompanyName"].ToString(),
                    id)
                {
                    AvailableFlowers = jo["AvailableFlowers"]?.ToObject<List<Flower>>() ?? new List<Flower>()
                },
                _ => throw new InvalidOperationException("Rôle inconnu")
            };

            return user;
        }

        public override void WriteJson(JsonWriter writer, Users value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Name");
            writer.WriteValue(value.Name);
            writer.WritePropertyName("Email");
            writer.WriteValue(value.Email);
            writer.WritePropertyName("ID");
            writer.WriteValue(value.ID);
            writer.WritePropertyName("Role");
            writer.WriteValue(value.Role);

            if (value is Supplier supplier)
            {
                writer.WritePropertyName("CompanyName");
                writer.WriteValue(supplier.CompanyName);
                writer.WritePropertyName("AvailableFlowers");
                serializer.Serialize(writer, supplier.AvailableFlowers);
            }

            writer.WriteEndObject();
        }
    }
}
