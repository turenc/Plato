﻿using System.Collections.Generic;
using System.Threading.Tasks;
using PlatoCore.Repositories;

namespace Plato.Entities.Repositories
{
    public interface IEntityRepository<T> : IRepository<T> where T : class
    {
        Task<IEnumerable<T>> SelectByFeatureIdAsync(int featureId);
    }

}
