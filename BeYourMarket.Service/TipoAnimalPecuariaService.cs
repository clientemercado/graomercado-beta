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
    public interface ITipoAnimalPecuariaService : IService<TipoAnimalPecuaria>
    {
    }

    public class TipoAnimalPecuariaService : Service<TipoAnimalPecuaria>, ITipoAnimalPecuariaService
    {
        public TipoAnimalPecuariaService(IRepositoryAsync<TipoAnimalPecuaria> repository)
            : base(repository)
        {
        }
    }
}
