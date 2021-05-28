﻿using System;
using System.Collections.Generic;
using System.Text;
using PnP.Core.Transformation.Model.Classic;

namespace PnP.Core.Transformation.Services.MappingProviders
{
    /// <summary>
    /// Defines the input for a URL mapping provider
    /// </summary>
    public class UserMappingProviderInput : MappingProviderInput
    {
        /// <summary>
        /// Defines the source UPN to map
        /// </summary>
        public string UserPrincipalName { get; set; }
    }
}