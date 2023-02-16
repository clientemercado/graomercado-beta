﻿using BeYourMarket.Model.Models;
using Repository.Pattern.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeYourMarket.Service
{
    public interface IPictureService : IService<Picture>
    {
    }

    public class PictureService : Service<Picture>, IPictureService
    {
        public PictureService(IRepositoryAsync<Picture> repository)
            : base(repository)
        {
        }
    }
}
