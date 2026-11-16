using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Models;

[Index("Nome", Name = "UQ_Album_Nome", IsUnique = true)]
public partial class Album
{
    [Key]
    public int IdAlbum { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Nome { get; set; } = null!;

    public DateOnly DataLancamento { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? Capa { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string? Descricao { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string TipoLancamento { get; set; } = null!;

    // [JsonIgnore] adicionado manualmente após o Scaffold (evita ciclo na serialização)
    [JsonIgnore]
    [InverseProperty("IdAlbumNavigation")]
    public virtual ICollection<Musica> Musica { get; set; } = new List<Musica>();
}
