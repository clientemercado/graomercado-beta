using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace BeYourMarket.Model.Models.Mapping
{
    public class AspNetUserMap : EntityTypeConfiguration<AspNetUser>
    {
        public AspNetUserMap()
        {
            // Primary Key
            this.HasKey(t => t.Id);

            // Properties
            this.Property(t => t.Id)
                .IsRequired()
                .HasMaxLength(128);

            this.Property(t => t.Email)
                .HasMaxLength(256);

            this.Property(t => t.UserName)
                .IsRequired()
                .HasMaxLength(256);

            // Table & Column Mappings
            this.ToTable("AspNetUsers");
            this.Property(t => t.Id).HasColumnName("Id");
            this.Property(t => t.FirstName).HasColumnName("FirstName");
            this.Property(t => t.LastName).HasColumnName("LastName");
            this.Property(t => t.RegisterDate).HasColumnName("RegisterDate");
            this.Property(t => t.RegisterIP).HasColumnName("RegisterIP");
            this.Property(t => t.LastAccessDate).HasColumnName("LastAccessDate");
            this.Property(t => t.LastAccessIP).HasColumnName("LastAccessIP");
            this.Property(t => t.DateOfBirth).HasColumnName("DateOfBirth");
            this.Property(t => t.AcceptEmail).HasColumnName("AcceptEmail");
            this.Property(t => t.Gender).HasColumnName("Gender");
            this.Property(t => t.LeadSourceID).HasColumnName("LeadSourceID");
            this.Property(t => t.Email).HasColumnName("Email");
            this.Property(t => t.EmailConfirmed).HasColumnName("EmailConfirmed");
            this.Property(t => t.PasswordHash).HasColumnName("PasswordHash");
            this.Property(t => t.SecurityStamp).HasColumnName("SecurityStamp");
            this.Property(t => t.PhoneNumber).HasColumnName("PhoneNumber");
            this.Property(t => t.PhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed");
            this.Property(t => t.TwoFactorEnabled).HasColumnName("TwoFactorEnabled");
            this.Property(t => t.LockoutEndDateUtc).HasColumnName("LockoutEndDateUtc");
            this.Property(t => t.LockoutEnabled).HasColumnName("LockoutEnabled");
            this.Property(t => t.AccessFailedCount).HasColumnName("AccessFailedCount");
            this.Property(t => t.UserName).HasColumnName("UserName");
            this.Property(t => t.Disabled).HasColumnName("Disabled");
            this.Property(t => t.Rating).HasColumnName("Rating");
            this.Property(t => t.id_TipoCadastro).HasColumnName("id_TipoCadastro");
            this.Property(t => t.id_UF).HasColumnName("id_UF");
            this.Property(t => t.id_Cidade).HasColumnName("id_Cidade");
            this.Property(t => t.Id_UBankDetails).HasColumnName("Id_UBankDetails");
            this.Property(t => t.Bairro_Cidade).HasColumnName("Bairro_Cidade");
            this.Property(t => t.Cep_Bairro_Cidade).HasColumnName("Cep_Bairro_Cidade");
            this.Property(t => t.Logradouro_Cidade).HasColumnName("Logradouro_Cidade");
            this.Property(t => t.Complemento_Endereco).HasColumnName("Complemento_Endereco");
            this.Property(t => t.PhoneNumberWhats).HasColumnName("PhoneNumberWhats");
            this.Property(t => t.DataUltimaAlteracao).HasColumnName("DataUltimaAlteracao");
            this.Property(t => t.IPEfetuouUltimaAlteracao).HasColumnName("IPEfetuouUltimaAlteracao");
        }
    }
}
