using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace OwnIdSdk.NetCore3.Configuration.Abstractions
{
    public interface IProfileConfiguration
    {
        Type ProfileModelType { get; }
        
        IReadOnlyList<ProfileFieldMetadata> ProfileFieldMetadata { get; }

        ValidateOptionsResult Validate();

        void BuildMetadata();
    }
}