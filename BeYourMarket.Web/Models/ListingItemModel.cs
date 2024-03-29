﻿using BeYourMarket.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BeYourMarket.Web.Models
{
    public class ListingItemModel : ListingUpdateModel
    {
        public List<Listing> ListingsOther { get; set; }
        public Listing ListingCurrent { get; set; }
        public string UrlPicture { get; set; }
        public List<PictureModel> Pictures { get; set; }
        public List<DateTime> DatesBooked { get; set; }
        public ApplicationUser User { get; set; }
        public List<ListingReview> ListingReviews { get; set; }

        //Adicionado por Edwilson Curti em 16/10/2022
        //OBS: Armazena o cód. da Categoria e o Link de Vídeo, registrado pelo ofertante
        public int CategoryID { get; set; }
        public string LinkCam { get; set; }
        public int? TpAcess { get; set; }

        public string dataRegistroUserPubl { get; set; }
        public List<ChatOferta> listaChatsOferta { get; set; }
    }
}