using System;
using Microsoft.Extensions.Configuration;

namespace SAPO.EnvironmentVar
{
    public class EnvironmentVars
    {
        public string Port { get; private set; }

        public EnvironmentVars(IConfiguration config)
        {
            Port = config["PORT_DEVICE"] ?? "4370";
        }   

    }
}