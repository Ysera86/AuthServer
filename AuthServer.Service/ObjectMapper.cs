using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Service
{
    public static class ObjectMapper
    {
        // ObjectMapper.Mapper diye çağırana kadar içerideki kod çalışıp da memorye yüklenmicek > Lazy ile Lazy Loading yaptık.
        // örn bi classın bir propertysinde çok nüyük data olabilir. ihtiyaç duymadıkça yüklenmesin isteriz. o zaman lazy loading yaparız.

        private static readonly Lazy<IMapper> lazy = new Lazy<IMapper>(() =>
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DtoMapper>();
            });
            return config.CreateMapper();

        });

        public static IMapper Mapper => lazy.Value;
    }
}
