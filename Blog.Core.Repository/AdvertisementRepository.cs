using Blog.Core.IRepository;
using Blog.Core.IRepository.UnitOfWork;
using Blog.Core.Model.Models;
using Blog.Core.Repository.Base;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Core.Repository
{
    public class AdvertisementRepository : BaseRepository<Advertisement>, IAdvertisementRepository
    {
        //public int Sum(int i, int j)
        //{
        //    return i + j;
        //}
        public AdvertisementRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
    }
}
