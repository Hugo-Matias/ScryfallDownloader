﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ScryfallDownloader.Services;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DownloaderContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.2");

            modelBuilder.Entity("ScryfallDownloader.Data.Artist", b =>
                {
                    b.Property<int>("ArtistId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ArtistId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Artists");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Author", b =>
                {
                    b.Property<int>("AuthorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("AuthorId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Card", b =>
                {
                    b.Property<int>("CardId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ArtistId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CollectorsNumber")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("ConvertedManaCost")
                        .HasColumnType("TEXT");

                    b.Property<string>("HandModifier")
                        .HasColumnType("TEXT");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsHighres")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsImplemented")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("LayoutId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LifeModifier")
                        .HasColumnType("TEXT");

                    b.Property<string>("Loyalty")
                        .HasColumnType("TEXT");

                    b.Property<string>("ManaCost")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Power")
                        .HasColumnType("TEXT");

                    b.Property<int?>("RarityId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SetId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Toughness")
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("CardId");

                    b.HasIndex("ArtistId");

                    b.HasIndex("LayoutId");

                    b.HasIndex("RarityId");

                    b.HasIndex("SetId");

                    b.HasIndex("Name", "SetId", "CollectorsNumber")
                        .IsUnique();

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.CardColor", b =>
                {
                    b.Property<int>("CardId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ColorId")
                        .HasColumnType("INTEGER");

                    b.HasKey("CardId", "ColorId");

                    b.HasIndex("ColorId");

                    b.ToTable("CardColors");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.CardGenerateColor", b =>
                {
                    b.Property<int>("CardId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ColorId")
                        .HasColumnType("INTEGER");

                    b.HasKey("CardId", "ColorId");

                    b.HasIndex("ColorId");

                    b.ToTable("CardGenerateColors");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.CardKeyword", b =>
                {
                    b.Property<int>("CardId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("KeywordId")
                        .HasColumnType("INTEGER");

                    b.HasKey("CardId", "KeywordId");

                    b.HasIndex("KeywordId");

                    b.ToTable("CardKeywords");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Color", b =>
                {
                    b.Property<int>("ColorId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Symbol")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ColorId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Colors");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Deck", b =>
                {
                    b.Property<int>("DeckId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Commander2CardId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CommanderCardId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("CommentCount")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreateDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<int>("FormatId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("LikeCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MissingCards")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SourceId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("UpdateDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("ViewCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("DeckId");

                    b.HasIndex("AuthorId");

                    b.HasIndex("Commander2CardId");

                    b.HasIndex("CommanderCardId");

                    b.HasIndex("FormatId");

                    b.HasIndex("SourceId");

                    b.ToTable("Decks");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.DeckCard", b =>
                {
                    b.Property<int>("DeckId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CardId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsSideboard")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Quantity")
                        .HasColumnType("INTEGER");

                    b.HasKey("DeckId", "CardId", "IsSideboard");

                    b.HasIndex("CardId");

                    b.ToTable("DeckCards");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.EdhrecCommander", b =>
                {
                    b.Property<int>("EdhrecCommanderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("DeckCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("EdhrecCommanderId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("EdhrecCommanders");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Format", b =>
                {
                    b.Property<int>("FormatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("FormatId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Formats");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Keyword", b =>
                {
                    b.Property<int>("KeywordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("KeywordId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Keywords");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Layout", b =>
                {
                    b.Property<int>("LayoutId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LayoutId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Layouts");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Rarity", b =>
                {
                    b.Property<int>("RarityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<char>("Symbol")
                        .HasColumnType("TEXT");

                    b.HasKey("RarityId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Rarities");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Set", b =>
                {
                    b.Property<int>("SetId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ForgeCode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<int?>("SetTypeId")
                        .HasColumnType("INTEGER");

                    b.HasKey("SetId");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("SetTypeId");

                    b.ToTable("Sets");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.SetType", b =>
                {
                    b.Property<int>("SetTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("SetTypeId");

                    b.ToTable("SetTypes");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Setting", b =>
                {
                    b.Property<int>("SettingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ARCHPage")
                        .HasColumnType("INTEGER");

                    b.Property<string>("EDHCommander")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("EDHDeck")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ImportSet")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MT8Page")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MissingCardDecks")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("SCGDate")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("SCGDeck")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SCGLimit")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SCGPage")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WTFDeck")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("SettingId");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Source", b =>
                {
                    b.Property<int>("SourceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("SourceId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Sources");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Tag", b =>
                {
                    b.Property<int>("TagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DeckId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("TagId");

                    b.HasIndex("DeckId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Card", b =>
                {
                    b.HasOne("ScryfallDownloader.Data.Artist", "Artist")
                        .WithMany()
                        .HasForeignKey("ArtistId");

                    b.HasOne("ScryfallDownloader.Data.Layout", "Layout")
                        .WithMany()
                        .HasForeignKey("LayoutId");

                    b.HasOne("ScryfallDownloader.Data.Rarity", "Rarity")
                        .WithMany()
                        .HasForeignKey("RarityId");

                    b.HasOne("ScryfallDownloader.Data.Set", "Set")
                        .WithMany("Cards")
                        .HasForeignKey("SetId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Artist");

                    b.Navigation("Layout");

                    b.Navigation("Rarity");

                    b.Navigation("Set");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.CardColor", b =>
                {
                    b.HasOne("ScryfallDownloader.Data.Card", "Card")
                        .WithMany("Colors")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ScryfallDownloader.Data.Color", "Color")
                        .WithMany()
                        .HasForeignKey("ColorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");

                    b.Navigation("Color");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.CardGenerateColor", b =>
                {
                    b.HasOne("ScryfallDownloader.Data.Card", "Card")
                        .WithMany("ProducedColors")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ScryfallDownloader.Data.Color", "Color")
                        .WithMany()
                        .HasForeignKey("ColorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");

                    b.Navigation("Color");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.CardKeyword", b =>
                {
                    b.HasOne("ScryfallDownloader.Data.Card", "Card")
                        .WithMany("Keywords")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ScryfallDownloader.Data.Keyword", "Keyword")
                        .WithMany()
                        .HasForeignKey("KeywordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");

                    b.Navigation("Keyword");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Deck", b =>
                {
                    b.HasOne("ScryfallDownloader.Data.Author", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ScryfallDownloader.Data.Card", "Commander2")
                        .WithMany()
                        .HasForeignKey("Commander2CardId");

                    b.HasOne("ScryfallDownloader.Data.Card", "Commander")
                        .WithMany()
                        .HasForeignKey("CommanderCardId");

                    b.HasOne("ScryfallDownloader.Data.Format", "Format")
                        .WithMany()
                        .HasForeignKey("FormatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ScryfallDownloader.Data.Source", "Source")
                        .WithMany()
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Author");

                    b.Navigation("Commander");

                    b.Navigation("Commander2");

                    b.Navigation("Format");

                    b.Navigation("Source");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.DeckCard", b =>
                {
                    b.HasOne("ScryfallDownloader.Data.Card", "Card")
                        .WithMany("Decks")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ScryfallDownloader.Data.Deck", "Deck")
                        .WithMany("Cards")
                        .HasForeignKey("DeckId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");

                    b.Navigation("Deck");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Set", b =>
                {
                    b.HasOne("ScryfallDownloader.Data.SetType", "SetType")
                        .WithMany()
                        .HasForeignKey("SetTypeId");

                    b.Navigation("SetType");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Tag", b =>
                {
                    b.HasOne("ScryfallDownloader.Data.Deck", null)
                        .WithMany("Tags")
                        .HasForeignKey("DeckId");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Card", b =>
                {
                    b.Navigation("Colors");

                    b.Navigation("Decks");

                    b.Navigation("Keywords");

                    b.Navigation("ProducedColors");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Deck", b =>
                {
                    b.Navigation("Cards");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("ScryfallDownloader.Data.Set", b =>
                {
                    b.Navigation("Cards");
                });
#pragma warning restore 612, 618
        }
    }
}
