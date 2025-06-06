using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Backend.Entities
{
    public class Client
    {
        [Key]
        public Guid Id { get; set; }
        public required string Nom { get; set; }
        public string Telephone { get; set; } ="";
        public string Fax { get; set; } ="";
        public string Adresse { get; set; }="";

        [JsonIgnore]
        public virtual ICollection<Livraison> Livraisons { get; set; } = new List<Livraison>();
    }
}