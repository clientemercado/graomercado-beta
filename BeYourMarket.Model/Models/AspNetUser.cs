using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeYourMarket.Model.Models
{
    public partial class AspNetUser : Repository.Pattern.Ef6.Entity
    {
        public AspNetUser()
        {
            this.AspNetUserClaims = new List<AspNetUserClaim>();
            this.AspNetUserLogins = new List<AspNetUserLogin>();
            this.Listings = new List<Listing>();
            this.Messages = new List<Message>();
            this.MessageParticipants = new List<MessageParticipant>();
            this.MessageReadStates = new List<MessageReadState>();
            this.OrdersProvider = new List<Order>();
            this.OrdersReceiver = new List<Order>();
            this.ListingReviewsUserFrom = new List<ListingReview>();
            this.ListingReviewsUserTo = new List<ListingReview>();
            this.AspNetRoles = new List<AspNetRole>();
        }

        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public System.DateTime RegisterDate { get; set; }
        public string RegisterIP { get; set; }
        public System.DateTime LastAccessDate { get; set; }
        public string LastAccessIP { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public bool AcceptEmail { get; set; }
        public string Gender { get; set; }
        public int LeadSourceID { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public Nullable<System.DateTime> LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string UserName { get; set; }
        public bool Disabled { get; set; }
        public double Rating { get; set; }
        public int id_TipoCadastro { get; set; }
        public int? id_UF { get; set; }
        public int? id_Cidade { get; set; }
        public string Logradouro_Cidade { get; set; }
        public string Bairro_Cidade { get; set; }
        public string Cep_Bairro_Cidade { get; set; }
        public int? Id_UBankDetails { get; set; }
        public string cpf_Usuario { get; set; }
        public System.DateTime? Data_Nascimento { get; set; }
        public string Complemento_Endereco { get; set; }
        public string PhoneNumberWhats { get; set; }
        public DateTime? DataUltimaAlteracao { get; set; }
        public string IPEfetuouUltimaAlteracao { get; set; }

        [ForeignKey("Id_UBankDetails")]
        public virtual UserBankDetails UserBankDetails { get; set; }

        public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual ICollection<Listing> Listings { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<MessageParticipant> MessageParticipants { get; set; }
        public virtual ICollection<MessageReadState> MessageReadStates { get; set; }
        public virtual ICollection<Order> OrdersProvider { get; set; }
        public virtual ICollection<Order> OrdersReceiver { get; set; }
        public virtual ICollection<ListingReview> ListingReviewsUserFrom { get; set; }
        public virtual ICollection<ListingReview> ListingReviewsUserTo { get; set; }
        public virtual ICollection<AspNetRole> AspNetRoles { get; set; }
    }
}
