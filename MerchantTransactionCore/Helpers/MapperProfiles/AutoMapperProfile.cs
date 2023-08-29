using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MerchantTransactionCore.Dtos.Global;
using AutoMapper;
using MerchantTransactionCore.Dtos.Models;
using System.Data;

namespace MerchantTransactionCore.Helpers.MapperProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<StoredProcedureReturnResponse<string>, TransactionQueryResponse>().ReverseMap();
            CreateMap<StoredProcedureReturnResponse<string>, DataBaseAuthenticationResponse>().ReverseMap();
            CreateMap<StoredProcedureReturnResponse<DataTable>, DataBaseRequeryResponse>().ReverseMap();
            CreateMap<StoredProcedureReturnResponse<DataTable>, DatabaseTransactionsResponse>().ReverseMap();
        }
    }
}
