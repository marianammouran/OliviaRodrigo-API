using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Models;

[Index("Nome", Name = "UQ_Genero_Nome", IsUnique = true)]
public partial class Genero
{
    [Key]
    public int IdGenero { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string Nome { get; set; } = null!;

    // [JsonIgnore] adicionado manualmente após o Scaffold (mesmo ajuste feito no Plot!)
    // para evitar ciclo Musica -> Genero -> Musica na serialização.
    [JsonIgnore]
    [InverseProperty("IdGeneroNavigation")]
    public virtual ICollection<Musica> Musica { get; set; } = new List<Musica>();
}
