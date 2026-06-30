using System;
using AzubiLog.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AzubiLog.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

            modelBuilder.Entity("AzubiLog.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id").HasColumnType("TEXT");
                    b.Property<int>("AccessFailedCount").HasColumnType("INTEGER");
                    b.Property<int>("AnnualVacationDays").ValueGeneratedOnAdd().HasColumnType("INTEGER").HasDefaultValue(30);
                    b.Property<string>("ClassName").IsRequired().HasMaxLength(80).HasColumnType("TEXT");
                    b.Property<string>("CompanyName").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
                    b.Property<string>("ConcurrencyStamp").IsConcurrencyToken().HasColumnType("TEXT");
                    b.Property<string>("Email").HasMaxLength(256).HasColumnType("TEXT");
                    b.Property<bool>("EmailConfirmed").HasColumnType("INTEGER");
                    b.Property<string>("FirstName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                    b.Property<bool>("IsActive").HasColumnType("INTEGER");
                    b.Property<string>("LastName").IsRequired().HasMaxLength(100).HasColumnType("TEXT");
                    b.Property<bool>("LockoutEnabled").HasColumnType("INTEGER");
                    b.Property<DateTimeOffset?>("LockoutEnd").HasColumnType("TEXT");
                    b.Property<string>("NormalizedEmail").HasMaxLength(256).HasColumnType("TEXT");
                    b.Property<string>("NormalizedUserName").HasMaxLength(256).HasColumnType("TEXT");
                    b.Property<string>("PasswordHash").HasColumnType("TEXT");
                    b.Property<string>("PhoneNumber").HasColumnType("TEXT");
                    b.Property<bool>("PhoneNumberConfirmed").HasColumnType("INTEGER");
                    b.Property<string>("School").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
                    b.Property<string>("SecurityStamp").HasColumnType("TEXT");
                    b.Property<string>("Subjects").IsRequired().HasMaxLength(500).HasColumnType("TEXT");
                    b.Property<bool>("TwoFactorEnabled").HasColumnType("INTEGER");
                    b.Property<string>("TrainerName").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
                    b.Property<string>("TrainingOccupation").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
                    b.Property<int>("TrainingYear").ValueGeneratedOnAdd().HasColumnType("INTEGER").HasDefaultValue(1);
                    b.Property<string>("UserName").HasMaxLength(256).HasColumnType("TEXT");
                    b.Property<double>("WeeklyTargetHours").ValueGeneratedOnAdd().HasColumnType("REAL").HasDefaultValue(40.0);

                    b.HasKey("Id");
                    b.HasIndex("NormalizedEmail").HasDatabaseName("EmailIndex");
                    b.HasIndex("NormalizedUserName").IsUnique().HasDatabaseName("UserNameIndex");
                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("AzubiLog.Models.CalendarDayMarker", b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                    b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
                    b.Property<DateOnly>("Date").HasColumnType("TEXT");
                    b.Property<string>("Note").HasMaxLength(1000).HasColumnType("TEXT");
                    b.Property<string>("Type").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
                    b.Property<DateTime?>("UpdatedAt").HasColumnType("TEXT");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

                    b.HasKey("Id");
                    b.HasIndex("UserId", "Date").IsUnique();
                    b.ToTable("CalendarDayMarkers", (string)null);
                });

            modelBuilder.Entity("AzubiLog.Models.Category", b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                    b.Property<string>("ColorHex").IsRequired().HasMaxLength(7).HasColumnType("TEXT");
                    b.Property<string>("Name").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
                    b.Property<int>("SortOrder").HasColumnType("INTEGER");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

                    b.HasKey("Id");
                    b.HasIndex("UserId", "Name").IsUnique();
                    b.ToTable("Categories", (string)null);
                });

            modelBuilder.Entity("AzubiLog.Models.ReportEntry", b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                    b.Property<int?>("CategoryId").HasColumnType("INTEGER");
                    b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
                    b.Property<DateTime>("Date").HasColumnType("TEXT");
                    b.Property<string>("DayType").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
                    b.Property<string>("Description").IsRequired().HasMaxLength(4000).HasColumnType("TEXT");
                    b.Property<decimal?>("Duration").HasPrecision(5, 2).HasColumnType("TEXT");
                    b.Property<DateTime>("EndTime").HasColumnType("TEXT");
                    b.Property<string>("Note").IsRequired().HasMaxLength(2000).HasColumnType("TEXT");
                    b.Property<string>("OrderNumber").HasMaxLength(80).HasColumnType("TEXT");
                    b.Property<DateTime>("StartTime").HasColumnType("TEXT");
                    b.Property<string>("Status").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
                    b.Property<string>("Subject").HasMaxLength(150).HasColumnType("TEXT");
                    b.Property<string>("Title").IsRequired().HasMaxLength(200).HasColumnType("TEXT");
                    b.Property<int?>("TrainerId").HasColumnType("INTEGER");
                    b.Property<DateTime>("UpdatedAt").HasColumnType("TEXT");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");
                    b.Property<int>("WeeklyReportId").HasColumnType("INTEGER");

                    b.HasKey("Id");
                    b.HasIndex("CategoryId");
                    b.HasIndex("TrainerId");
                    b.HasIndex("UserId", "Date");
                    b.HasIndex("WeeklyReportId");
                    b.ToTable("ReportEntries", (string)null);
                });

            modelBuilder.Entity("AzubiLog.Models.SchoolScheduleDay", b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                    b.Property<string>("DayOfWeek").IsRequired().HasMaxLength(20).HasColumnType("TEXT");
                    b.Property<string>("SubjectsText").IsRequired().HasMaxLength(1000).HasColumnType("TEXT");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

                    b.HasKey("Id");
                    b.HasIndex("UserId", "DayOfWeek").IsUnique();
                    b.ToTable("SchoolScheduleDays", (string)null);
                });

            modelBuilder.Entity("AzubiLog.Models.TodoItem", b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                    b.Property<DateTime?>("CompletedAt").HasColumnType("TEXT");
                    b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
                    b.Property<string>("Description").IsRequired().HasMaxLength(2000).HasColumnType("TEXT");
                    b.Property<DateTime?>("DueDate").HasColumnType("TEXT");
                    b.Property<bool>("IsCompleted").HasColumnType("INTEGER");
                    b.Property<string>("Title").IsRequired().HasMaxLength(200).HasColumnType("TEXT");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

                    b.HasKey("Id");
                    b.HasIndex("UserId", "IsCompleted", "DueDate");
                    b.ToTable("Todos", (string)null);
                });

            modelBuilder.Entity("AzubiLog.Models.Trainer", b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                    b.Property<string>("Department").IsRequired().HasMaxLength(120).HasColumnType("TEXT");
                    b.Property<string>("Email").IsRequired().HasMaxLength(256).HasColumnType("TEXT");
                    b.Property<string>("Name").IsRequired().HasMaxLength(150).HasColumnType("TEXT");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

                    b.HasKey("Id");
                    b.HasIndex("Email");
                    b.HasIndex("UserId", "Name");
                    b.ToTable("Trainers", (string)null);
                });

            modelBuilder.Entity("AzubiLog.Models.WeeklyReport", b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                    b.Property<int>("CalendarWeek").HasColumnType("INTEGER");
                    b.Property<string>("Comment").IsRequired().HasMaxLength(2000).HasColumnType("TEXT");
                    b.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
                    b.Property<string>("Status").IsRequired().HasMaxLength(40).HasColumnType("TEXT");
                    b.Property<double>("TotalHours").HasColumnType("REAL");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");
                    b.Property<int>("Year").HasColumnType("INTEGER");

                    b.HasKey("Id");
                    b.HasIndex("UserId", "Year", "CalendarWeek").IsUnique();
                    b.ToTable("WeeklyReports", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
                    b.Property<string>("ClaimType").HasColumnType("TEXT");
                    b.Property<string>("ClaimValue").HasColumnType("TEXT");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

                    b.HasKey("Id");
                    b.HasIndex("UserId");
                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider").HasColumnType("TEXT");
                    b.Property<string>("ProviderKey").HasColumnType("TEXT");
                    b.Property<string>("ProviderDisplayName").HasColumnType("TEXT");
                    b.Property<string>("UserId").IsRequired().HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");
                    b.HasIndex("UserId");
                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId").HasColumnType("TEXT");
                    b.Property<string>("LoginProvider").HasColumnType("TEXT");
                    b.Property<string>("Name").HasColumnType("TEXT");
                    b.Property<string>("Value").HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");
                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("AzubiLog.Models.Category", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", "User")
                        .WithMany("Categories")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AzubiLog.Models.CalendarDayMarker", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", "User")
                        .WithMany("CalendarDayMarkers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AzubiLog.Models.ReportEntry", b =>
                {
                    b.HasOne("AzubiLog.Models.Category", "Category")
                        .WithMany("ReportEntries")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("AzubiLog.Models.Trainer", "Trainer")
                        .WithMany("ReportEntries")
                        .HasForeignKey("TrainerId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("AzubiLog.Models.ApplicationUser", "User")
                        .WithMany("ReportEntries")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AzubiLog.Models.WeeklyReport", "WeeklyReport")
                        .WithMany("ReportEntries")
                        .HasForeignKey("WeeklyReportId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");
                    b.Navigation("Trainer");
                    b.Navigation("User");
                    b.Navigation("WeeklyReport");
                });

            modelBuilder.Entity("AzubiLog.Models.SchoolScheduleDay", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", "User")
                        .WithMany("SchoolScheduleDays")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AzubiLog.Models.TodoItem", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", "User")
                        .WithMany("Todos")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AzubiLog.Models.Trainer", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", "User")
                        .WithMany("Trainers")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AzubiLog.Models.WeeklyReport", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", "User")
                        .WithMany("WeeklyReports")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("AzubiLog.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AzubiLog.Models.ApplicationUser", b =>
                {
                    b.Navigation("CalendarDayMarkers");
                    b.Navigation("Categories");
                    b.Navigation("ReportEntries");
                    b.Navigation("SchoolScheduleDays");
                    b.Navigation("Todos");
                    b.Navigation("Trainers");
                    b.Navigation("WeeklyReports");
                });

            modelBuilder.Entity("AzubiLog.Models.Category", b =>
                {
                    b.Navigation("ReportEntries");
                });

            modelBuilder.Entity("AzubiLog.Models.Trainer", b =>
                {
                    b.Navigation("ReportEntries");
                });

            modelBuilder.Entity("AzubiLog.Models.WeeklyReport", b =>
                {
                    b.Navigation("ReportEntries");
                });
#pragma warning restore 612, 618
        }
    }
}
