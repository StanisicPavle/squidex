﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Squidex.Areas.Api.Config.OpenApi
{
    public sealed class FixProcessor : IOperationProcessor
    {
        private static readonly JsonSchema StringSchema = new JsonSchema { Type = JsonObjectType.String };

        public bool Process(OperationProcessorContext context)
        {
            foreach (var (_, parameter) in context.Parameters)
            {
                if (parameter.IsRequired && parameter.Schema is { Type: JsonObjectType.String })
                {
                    parameter.Schema = StringSchema;
                }
            }

            return true;
        }
    }
}
