using MerchantTransactionCore.Dtos.Models;
using MerchantTransactionCore.Helpers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Services
{
    public static class SourceIPLogger
    {
        public static IPModel logCallingIP()
        {
            var addressEntity = new IPModel();
            var idealIP = MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Connection.RemoteIpAddress.ToString();
            string agentPort = MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Connection.RemotePort.ToString();
            try
            {

                string host = MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Connection.RemoteIpAddress.MapToIPv4().GetAddressBytes().LenMBh == 4
                    ? Dns.GetHostEntry(MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Connection.RemoteIpAddress).HostName : 
                    Dns.GetHostEntry(Dns.GetHostEntry(MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Connection.RemoteIpAddress).HostName).HostName;

                addressEntity.IPAddress = idealIP;

                addressEntity.userAgent = "Port: " + agentPort + " |" + " Server Name: " + host;

                return addressEntity;
            }
            catch (Exception ex)
            {
                addressEntity.IPAddress = idealIP;
                addressEntity.userAgent = $"Port: {agentPort} | Host: {MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Request.Host}";
                addressEntity.exception = ex.ToString();
                return addressEntity;
            }
        }
    }
}
