using System;
using System.Collections.Generic;
using DiarioRoxo.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DiarioRoxo.WebAPI.Context;

public partial class DiarioRoxoDbContext : DbContext
{
    public DiarioRoxoDbContext()
    {
    }

    public DiarioRoxoDbContext(DbContextOptions<DiarioRoxoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Album> Album { get; set; }

    public virtual DbSet<Genero> Genero { get; set; }

    public virtual DbSet<Musica> Musica { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Album>(entity =>
        {
            entity.HasKey(e => e.IdAlbum).HasName("PK_Album");
        });

        modelBuilder.Entity<Genero>(entity =>
        {
            entity.HasKey(e => e.IdGenero).HasName("PK_Genero");
        });

        modelBuilder.Entity<Musica>(entity =>
        {
            entity.HasKey(e => e.IdMusica).HasName("PK_Musica");

            entity.HasOne(d => d.IdAlbumNavigation).WithMany(p => p.Musica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Musica_Album");

            entity.HasOne(d => d.IdGeneroNavigation).WithMany(p => p.Musica)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Musica_Genero");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK_Usuario");

            entity.ToTable(tb => tb.HasTrigger("TR_UsuarioSoftDelete"));

            entity.Property(e => e.Ativo).HasDefaultValue(true);
            entity.Property(e => e.DataCadastro).HasDefaultValueSql("(sysdatetime())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
