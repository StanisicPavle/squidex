﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.Serialization;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Web.Json;

namespace Squidex.Areas.Api.Controllers.Rules.Models
{
    [JsonInheritanceConverter(typeof(RuleTriggerDto), "triggerType")]
    [KnownType(nameof(Subtypes))]
    public abstract class RuleTriggerDto
    {
        public abstract RuleTrigger ToTrigger();

        public static Type[] Subtypes()
        {
            var type = typeof(RuleTriggerDto);

            return type.Assembly.GetTypes().Where(type.IsAssignableFrom).ToArray();
        }
    }
}
