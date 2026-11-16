using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Models;

[Index("IdAlbum", "NumeroFaixa", Name = "UQ_Musica_Album_Faixa", IsUnique = true)]
[Index("IdAlbum", "Titulo", Name = "UQ_Musica_Album_Titulo", IsUnique = true)]
public partial class Musica
{
    [Key]
    public int IdMusica { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string Titulo { get; set; } = null!;

    public int IdAlbum { get; set; }

    public int IdGenero { get; set; }

    public int NumeroFaixa { get; set; }

    public DateOnly DataLancamento { get; set; }

    public int Duracao { get; set; }

    [StringLength(500)]
    [Unicode(false)]
    public string? ImagemCapa { get; set; }

    [StringLength(1000)]
    [Unicode(false)]
    public string? Descricao { get; set; }

    [ForeignKey("IdAlbum")]
    [InverseProperty("Musica")]
    public virtual Album IdAlbumNavigation { get; set; } = null!;

    [ForeignKey("IdGenero")]
    [InverseProperty("Musica")]
    public virtual Genero IdGeneroNavigation { get; set; } = null!;
}
