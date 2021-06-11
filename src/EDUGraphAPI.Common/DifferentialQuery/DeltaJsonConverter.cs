/*   
 *   * Copyright (c) Microsoft Corporation. All rights reserved. Licensed under the MIT license.  
 *   * See LICENSE in the project root for license information.  
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EDUGraphAPI.DifferentialQuery
{
    public class DeltaJsonConverter<TEntity> : JsonConverter where TEntity : class, new()
    {
        // ReSharper disable once StaticMemberInGenericType - should not be shared
        private static Dictionary<string, PropertyInfo> EntityPropertyLookup;

        public override bool CanConvert(Type objectType)
        {
            return objectType.Name.Equals("Delta`1", StringComparison.OrdinalIgnoreCase);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Delta<TEntity> existingDelta = existingValue as Delta<TEntity>;

            if (existingValue != null && existingDelta == null)
            {
                throw new JsonSerializationException("Unexpected object type.");
            }
            else if (existingValue == null)
            {
                existingDelta = new Delta<TEntity>(new TEntity());
            }

            JObject obj = JObject.Load(reader);
            JToken theValue;

            if (obj.TryGetValue("@removed", out theValue))
            {
                existingDelta.Removed = theValue.ToObject<DeltaRemovedData>(serializer);
                existingDelta.Id = obj.Property("id").Value.ToObject<string>(serializer);
            }
            else
            {
                if (EntityPropertyLookup == null)
                {
                    EntityPropertyLookup =
                        typeof(TEntity)
                            .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                            .ToDictionary(p => ToLowerCamel(p.Name),
                                p => p,
                                StringComparer.OrdinalIgnoreCase);
                }

                // Update the strongly-typed Entity with anything that is in the Json. and record that it was changed
                // so it can be mapped to the database record later.
                foreach (var entry in EntityPropertyLookup)
                {
                    if (obj.TryGetValue(entry.Key, out theValue))
                    {
                        entry.Value.SetValue(existingDelta.Entity,
                            theValue.ToObject(entry.Value.PropertyType, serializer));
                        existingDelta.ModifiedProperties[entry.Value.Name] = theValue;
                    }
                }
            }

            return existingDelta;
        }

        private string ToLowerCamel(string input)
        {
            return input.First().ToString().ToLowerInvariant() + input.Substring(1);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}